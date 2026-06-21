using System.Runtime.InteropServices;
using UnityEngine;

public class MeshSurfaceMover : MonoBehaviour
{
    [Header("Skinned Mesh Setup")]
    public MeshEdgeScanner scanner; 
    public SkinnedMeshRenderer skinnedMeshRenderer; 
    public InputReader input;
    
    [Header("Variables")]
    public float moveSpeed = 5.0f;

    [Header("Current State")]
    public int currentTriangleIndex = 0;
    public Vector3 barycentricCoords = new Vector3(0.333f, 0.333f, 0.334f); 

    private Mesh bakedMesh; 
    private Vector3[] vertices;
    private int[] triangles;

    private Vector2 movementVector = Vector2.zero;
    private bool debugMoving = true; 

    void OnDestroy()
    {
        if (input != null)
        {
            input.MoveEvent -= HandleMove;
        }
    }

    void Start()
    {
        if (scanner == null)
        {
            Debug.LogError("[MeshSurfaceMover] Scanner reference is missing!", this);
            return;
        }

        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("[MeshSurfaceMover] SkinnedMeshRenderer reference is missing! Please assign it.", this);
            return;
        }

        if (input != null)
        {
            input.MoveEvent += HandleMove;
        }
        else
        {
            Debug.LogError("[MeshSurfaceMover] InputReader reference is missing!", this);
        }
        
        bakedMesh = new Mesh();
        triangles = skinnedMeshRenderer.sharedMesh.triangles;
        
        UpdateBakedVertices();
        UpdatePhysicalPosition();
    }

    void Update()
    {
        UpdateBakedVertices();

        if (movementVector.sqrMagnitude > 0.001f)
        {
            MoveOnSurface(movementVector, moveSpeed);
            if (!debugMoving)
            {
                debugMoving = true;
                Debug.Log("moving");
            }
        }
        else
        {
            UpdatePhysicalPosition();
            if (debugMoving)
            {
                debugMoving = false;
                Debug.Log("locked");
            }
        }
    }

    private void UpdateBakedVertices()
    {
        // Force BakeMesh to parse true clean vertices ignoring outer game object scale multipliers
        skinnedMeshRenderer.BakeMesh(bakedMesh, true);
        vertices = bakedMesh.vertices;
    }

    private bool IsTriangleValid(int triangleIndex)
    {
        if (triangles == null || vertices == null) return false;
        
        int tBase = triangleIndex * 3;
        if (tBase + 2 >= triangles.Length || tBase < 0) return false;

        int idxA = triangles[tBase];
        int idxB = triangles[tBase + 1];
        int idxC = triangles[tBase + 2];

        if (idxA >= vertices.Length || idxB >= vertices.Length || idxC >= vertices.Length) return false;
        if (idxA < 0 || idxB < 0 || idxC < 0) return false;

        return true;
    }

    // FIXED MATRIX TRANSLATION MODULE: Combines the unscaled BakeMesh snapshot coordinates 
    // with the true physical lossyScale and position vectors of the renderer's workspace frame.
    private Vector3 GetTrueWorldVertex(int vertexIndex)
    {
        Transform smrTransform = skinnedMeshRenderer.transform;
        
        // Build a custom TRS matrix that incorporates parent scaling but 
        // completely filters out the double-multiplication errors caused by SMR.
        Matrix4x4 trueScaleMatrix = Matrix4x4.TRS(
            smrTransform.position, 
            smrTransform.rotation, 
            smrTransform.lossyScale
        );
                                
        return trueScaleMatrix.MultiplyPoint3x4(vertices[vertexIndex]);
    }

    public void MoveOnSurface(Vector2 moveInput, float speed)
    {
        if (scanner.triangleNeighbors == null || currentTriangleIndex >= scanner.triangleNeighbors.Length) 
        {
            Debug.LogWarning("[MeshSurfaceMover] Neighbors map isn't built yet.");
            return;
        }

        if (!IsTriangleValid(currentTriangleIndex))
        {
            Debug.LogWarning($"[MeshSurfaceMover] Triangle index {currentTriangleIndex} has broken vertices. Resetting to 0.");
            currentTriangleIndex = 0;
            if (!IsTriangleValid(currentTriangleIndex)) return; 
        }

        int tBase = currentTriangleIndex * 3;
        Vector3 worldA = GetTrueWorldVertex(triangles[tBase]);
        Vector3 worldB = GetTrueWorldVertex(triangles[tBase + 1]);
        Vector3 worldC = GetTrueWorldVertex(triangles[tBase + 2]);

        Vector3 surfaceNormal = Vector3.Cross(worldB - worldA, worldC - worldA).normalized;

        Vector3 worldClimbUp = Vector3.up - Vector3.Project(Vector3.up, surfaceNormal);
        if (worldClimbUp.sqrMagnitude < 0.001f)
        {
            worldClimbUp = Vector3.forward - Vector3.Project(Vector3.forward, surfaceNormal);
        }
        worldClimbUp.Normalize();

        Vector3 worldClimbRight = Vector3.Cross(worldClimbUp, surfaceNormal).normalized;

        Vector3 worldMoveDelta = ((worldClimbRight * moveInput.x) + (worldClimbUp * moveInput.y)) * speed * Time.deltaTime;

        Vector3 currentWorldPos = worldA * barycentricCoords.x + worldB * barycentricCoords.y + worldC * barycentricCoords.z;
        Vector3 targetWorldPos = currentWorldPos + worldMoveDelta;

        Vector3 newBarycentric = CalculateBarycentricWorld(targetWorldPos, worldA, worldB, worldC);

        if (newBarycentric.x >= -0.02f && newBarycentric.y >= -0.02f && newBarycentric.z >= -0.02f)
        {
            barycentricCoords.x = Mathf.Max(0f, newBarycentric.x);
            barycentricCoords.y = Mathf.Max(0f, newBarycentric.y);
            barycentricCoords.z = Mathf.Max(0f, newBarycentric.z);
            float total = barycentricCoords.x + barycentricCoords.y + barycentricCoords.z;
            if (total > 0f) barycentricCoords /= total;
        }
        else
        {
            HandleTriangleTransitionWorld(targetWorldPos, newBarycentric);
        }

        UpdatePhysicalPosition();
    }

    private void HandleTriangleTransitionWorld(Vector3 targetWorldPos, Vector3 failedBarycentric)
    {
        var neighbors = scanner.triangleNeighbors[currentTriangleIndex];

        int bestNeighbor = -1;
        float bestDistance = float.MaxValue;

        if (neighbors != null && neighbors.Count > 0)
        {
            foreach (int neighborIndex in neighbors)
            {
                if (!IsTriangleValid(neighborIndex)) continue;

                int nBase = neighborIndex * 3;
                Vector3 worldA = GetTrueWorldVertex(triangles[nBase]);
                Vector3 worldB = GetTrueWorldVertex(triangles[nBase + 1]);
                Vector3 worldC = GetTrueWorldVertex(triangles[nBase + 2]);

                Vector3 neighborBary = CalculateBarycentricWorld(targetWorldPos, worldA, worldB, worldC);
                
                float outOfBoundsPenalty = Mathf.Max(0, -neighborBary.x) + Mathf.Max(0, -neighborBary.y) + Mathf.Max(0, -neighborBary.z);
                if (outOfBoundsPenalty < bestDistance)
                {
                    bestDistance = outOfBoundsPenalty;
                    bestNeighbor = neighborIndex;
                }
            }
        }

        if (bestNeighbor != -1 && bestDistance < 0.5f && IsTriangleValid(bestNeighbor))
        {
            currentTriangleIndex = bestNeighbor;
            
            int nBase = currentTriangleIndex * 3;
            Vector3 worldA = GetTrueWorldVertex(triangles[nBase]);
            Vector3 worldB = GetTrueWorldVertex(triangles[nBase + 1]);
            Vector3 worldC = GetTrueWorldVertex(triangles[nBase + 2]);

            Vector3 finalBary = CalculateBarycentricWorld(targetWorldPos, worldA, worldB, worldC);
            
            barycentricCoords.x = Mathf.Max(0, finalBary.x);
            barycentricCoords.y = Mathf.Max(0, finalBary.y);
            barycentricCoords.z = Mathf.Max(0, finalBary.z);
        }
        else
        {
            barycentricCoords.x = Mathf.Max(0, failedBarycentric.x);
            barycentricCoords.y = Mathf.Max(0, failedBarycentric.y);
            barycentricCoords.z = Mathf.Max(0, failedBarycentric.z);
        }

        float total = barycentricCoords.x + barycentricCoords.y + barycentricCoords.z;
        if (total > 0) barycentricCoords /= total;
    }

    private void UpdatePhysicalPosition()
    {
        if (vertices == null || triangles == null || vertices.Length == 0) return;
        if (!IsTriangleValid(currentTriangleIndex)) return; 

        int tBase = currentTriangleIndex * 3;
        Vector3 worldA = GetTrueWorldVertex(triangles[tBase]);
        Vector3 worldB = GetTrueWorldVertex(triangles[tBase + 1]);
        Vector3 worldC = GetTrueWorldVertex(triangles[tBase + 2]);

        // Place directly on the true synchronized surface mesh points
        transform.position = worldA * barycentricCoords.x + worldB * barycentricCoords.y + worldC * barycentricCoords.z;

        Vector3 surfaceNormal = Vector3.Cross(worldB - worldA, worldC - worldA).normalized;

        if (surfaceNormal.sqrMagnitude > 0.001f)
        {
            Vector3 visualForward = Vector3.up - Vector3.Project(Vector3.up, surfaceNormal);
            if (visualForward.sqrMagnitude < 0.001f)
            {
                visualForward = Vector3.forward - Vector3.Project(Vector3.forward, surfaceNormal);
            }

            visualForward.Normalize();
            transform.rotation = Quaternion.LookRotation(visualForward, surfaceNormal);
        }
    }


    private Vector3 CalculateBarycentricWorld(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        if (Mathf.Abs(denom) < 0.0001f) return new Vector3(0.33f, 0.33f, 0.34f);
        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;
        
        return new Vector3(u, v, w);
    }

    private void HandleMove(Vector2 inputAxis)
    {
        movementVector = inputAxis;
    }
}
