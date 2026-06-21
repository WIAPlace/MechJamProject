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

    void OnDestroy()
    {
        if (input != null)
        {
            input.MoveEvent -= HandleMove;
        }
    }

    void Start()
    {
        if (scanner == null || skinnedMeshRenderer == null)
        {
            Debug.LogError("[MeshSurfaceMover] Missing critical setup links in inspector!", this);
            return;
        }

        if (input != null) input.MoveEvent += HandleMove;
        
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
            MoveOnSurfaceParametric(movementVector, moveSpeed);
        }
        else
        {
            UpdatePhysicalPosition();
        }
    }

    private void UpdateBakedVertices()
    {
        skinnedMeshRenderer.BakeMesh(bakedMesh, true);
        vertices = bakedMesh.vertices;
    }

    private bool IsTriangleValid(int triangleIndex)
    {
        if (triangles == null || vertices == null) return false;
        int tBase = triangleIndex * 3;
        if (tBase + 2 >= triangles.Length || tBase < 0) return false;
        return true;
    }

    private Vector3 GetTrueWorldVertex(int vertexIndex)
    {
        Transform smrTransform = skinnedMeshRenderer.transform;
        Matrix4x4 trueScaleMatrix = Matrix4x4.TRS(smrTransform.position, smrTransform.rotation, smrTransform.lossyScale);
        return trueScaleMatrix.MultiplyPoint3x4(vertices[vertexIndex]);
    }

    public void MoveOnSurfaceParametric(Vector2 moveInput, float speed)
    {
        if (scanner.triangleNeighbors == null || !IsTriangleValid(currentTriangleIndex)) return;

        int tBase = currentTriangleIndex * 3;
        Vector3 worldA = GetTrueWorldVertex(triangles[tBase]);
        Vector3 worldB = GetTrueWorldVertex(triangles[tBase + 1]);
        Vector3 worldC = GetTrueWorldVertex(triangles[tBase + 2]);

        Vector3 triNormal = Vector3.Cross(worldB - worldA, worldC - worldA).normalized;
        Vector3 worldClimbUp = Vector3.up - Vector3.Project(Vector3.up, triNormal);
        if (worldClimbUp.sqrMagnitude < 0.001f)
        {
            worldClimbUp = Vector3.forward - Vector3.Project(Vector3.forward, triNormal);
        }
        worldClimbUp.Normalize();
        Vector3 worldClimbRight = Vector3.Cross(worldClimbUp, triNormal).normalized;

        Vector3 velocityWorld = (worldClimbRight * -moveInput.x + worldClimbUp * moveInput.y) * speed;
        float remainingTime = Time.deltaTime;

        int loopCount = 0;
        while (remainingTime > 0.0001f && loopCount < 4)
        {
            loopCount++;
            tBase = currentTriangleIndex * 3;
            worldA = GetTrueWorldVertex(triangles[tBase]);
            worldB = GetTrueWorldVertex(triangles[tBase + 1]);
            worldC = GetTrueWorldVertex(triangles[tBase + 2]);
            triNormal = Vector3.Cross(worldB - worldA, worldC - worldA).normalized;

            Vector3 currentPosWorld = worldA * barycentricCoords.x + worldB * barycentricCoords.y + worldC * barycentricCoords.z;
            Vector3 stepDeltaWorld = velocityWorld * remainingTime;
            Vector3 targetPosWorld = currentPosWorld + stepDeltaWorld;

            Vector3 testBary = CalculateBarycentricWorld(targetPosWorld, worldA, worldB, worldC);

            if (testBary.x >= 0f && testBary.y >= 0f && testBary.z >= 0f)
            {
                barycentricCoords = testBary;
                remainingTime = 0f;
                break;
            }

            int crossedEdge = 0; 
            float minCoord = testBary.x;
            if (testBary.y < minCoord) { minCoord = testBary.y; crossedEdge = 1; }
            if (testBary.z < minCoord) { minCoord = testBary.z; crossedEdge = 2; }

            var neighbors = scanner.triangleNeighbors[currentTriangleIndex];
            int nextTriangle = -1;
            float bestDistance = float.MaxValue;

            if (neighbors != null)
            {
                foreach (int nIdx in neighbors)
                {
                    if (!IsTriangleValid(nIdx)) continue;
                    int nBase = nIdx * 3;
                    Vector3 nA = GetTrueWorldVertex(triangles[nBase]);
                    Vector3 nB = GetTrueWorldVertex(triangles[nBase + 1]);
                    Vector3 nC = GetTrueWorldVertex(triangles[nBase + 2]);

                    Vector3 nBary = CalculateBarycentricWorld(targetPosWorld, nA, nB, nC);
                    float penalty = Mathf.Max(0, -nBary.x) + Mathf.Max(0, -nBary.y) + Mathf.Max(0, -nBary.z);
                    if (penalty < bestDistance)
                    {
                        bestDistance = penalty;
                        nextTriangle = nIdx;
                    }
                }
            }

            if (nextTriangle != -1 && bestDistance < 0.5f)
            {
                float tFactor = 0f;
                if (minCoord < 0f && (1f - minCoord) > 0f)
                {
                    tFactor = Mathf.Clamp01(1f / (1f - minCoord));
                }

                Vector3 edgeBary = barycentricCoords + (testBary - barycentricCoords) * tFactor;
                edgeBary.x = Mathf.Clamp01(edgeBary.x);
                edgeBary.y = Mathf.Clamp01(edgeBary.y);
                edgeBary.z = Mathf.Clamp01(edgeBary.z);
                float sum = edgeBary.x + edgeBary.y + edgeBary.z;
                if (sum > 0f) edgeBary /= sum;

                Vector3 boundaryPosWorld = worldA * edgeBary.x + worldB * edgeBary.y + worldC * edgeBary.z;
                currentTriangleIndex = nextTriangle;

                int nextBase = currentTriangleIndex * 3;
                Vector3 nA = GetTrueWorldVertex(triangles[nextBase]);
                Vector3 nB = GetTrueWorldVertex(triangles[nextBase + 1]);
                Vector3 nC = GetTrueWorldVertex(triangles[nextBase + 2]);
                Vector3 nextNormal = Vector3.Cross(nB - nA, nC - nA).normalized;

                Quaternion planeRotation = Quaternion.FromToRotation(triNormal, nextNormal);
                velocityWorld = planeRotation * velocityWorld;

                barycentricCoords = CalculateBarycentricWorld(boundaryPosWorld, nA, nB, nC);
                remainingTime -= remainingTime * tFactor;
            }
            else
            {
                barycentricCoords.x = Mathf.Clamp01(testBary.x);
                barycentricCoords.y = Mathf.Clamp01(testBary.y);
                barycentricCoords.z = Mathf.Clamp01(testBary.z);
                float total = barycentricCoords.x + barycentricCoords.y + barycentricCoords.z;
                if (total > 0f) barycentricCoords /= total;
                remainingTime = 0f;
            }
        }

        UpdatePhysicalPosition();
    }
    private void UpdatePhysicalPosition()
    {
        if (vertices == null || triangles == null || vertices.Length == 0) return;
        if (!IsTriangleValid(currentTriangleIndex)) return; 

        int tBase = currentTriangleIndex * 3;
        Vector3 worldA = GetTrueWorldVertex(triangles[tBase]);
        Vector3 worldB = GetTrueWorldVertex(triangles[tBase + 1]);
        Vector3 worldC = GetTrueWorldVertex(triangles[tBase + 2]);

        transform.position = worldA * barycentricCoords.x + worldB * barycentricCoords.y + worldC * barycentricCoords.z;

        Vector3 smoothNormal = GetSmoothedSurfaceNormal(currentTriangleIndex, barycentricCoords);

        if (smoothNormal.sqrMagnitude > 0.001f)
        {
            Vector3 visualForward = Vector3.up - Vector3.Project(Vector3.up, smoothNormal);
            if (visualForward.sqrMagnitude < 0.001f)
            {
                visualForward = Vector3.forward - Vector3.Project(Vector3.forward, smoothNormal);
            }

            visualForward.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(visualForward, smoothNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 30.0f * Time.deltaTime);
        }
    }

    private Vector3 GetSmoothedSurfaceNormal(int triangleIndex, Vector3 weights)
    {
        if (bakedMesh == null) return Vector3.up;

        int tBase = triangleIndex * 3;
        int idxA = triangles[tBase];
        int idxB = triangles[tBase + 1];
        int idxC = triangles[tBase + 2];

        Vector3[] meshNormals = bakedMesh.normals;
        if (meshNormals == null || meshNormals.Length <= idxC)
        {
            Vector3 worldA = GetTrueWorldVertex(idxA);
            Vector3 worldB = GetTrueWorldVertex(idxB);
            Vector3 worldC = GetTrueWorldVertex(idxC);
            return Vector3.Cross(worldB - worldA, worldC - worldA).normalized;
        }

        Vector3 blendedLocalNormal = (meshNormals[idxA] * weights.x) + (meshNormals[idxB] * weights.y) + (meshNormals[idxC] * weights.z);
        return skinnedMeshRenderer.transform.TransformDirection(blendedLocalNormal).normalized;
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