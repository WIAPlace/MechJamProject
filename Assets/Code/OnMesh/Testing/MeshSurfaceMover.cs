using System.Runtime.InteropServices;
using UnityEngine;

public class MeshSurfaceMover : MonoBehaviour
{
    public MeshEdgeScanner scanner; // Reference to your script
    public InputReader input;
    [Header("Variables")]
    public float moveSpeed = 5.0f;

    [Header("Current State")]
    public int currentTriangleIndex = 0;
    public Vector3 barycentricCoords = new Vector3(0.333f, 0.333f, 0.334f); // Start in center

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

        MeshFilter mf = scanner.GetComponent<MeshFilter>();
        if (mf == null || mf.mesh == null)
        {
            Debug.LogError("[MeshSurfaceMover] The Scanner object does not have a valid MeshFilter component!", this);
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
        
        vertices = mf.mesh.vertices;
        triangles = mf.mesh.triangles;

        UpdatePhysicalPosition();
    }

    void Update()
    {
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

    public void MoveOnSurface(Vector2 moveInput, float speed)
    {
        if (scanner.triangleNeighbors == null || currentTriangleIndex >= scanner.triangleNeighbors.Length) 
        {
            Debug.LogWarning("[MeshSurfaceMover] Neighbors map isn't built yet.");
            return;
        }

        Transform meshTransform = scanner.transform;
        Matrix4x4 localToWorld = meshTransform.localToWorldMatrix;

        // 1. Fetch raw vertices and project them into World Space
        int tBase = currentTriangleIndex * 3;
        Vector3 worldA = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase]]);
        Vector3 worldB = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase + 1]]);
        Vector3 worldC = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase + 2]]);

        // 2. Get the true World-Space normal of the current triangle face
        Vector3 surfaceNormal = Vector3.Cross(worldB - worldA, worldC - worldA).normalized;

        // 3. Create a world-relative movement frame on the triangle's surface plane.
        // Pushing 'W' (moveInput.y) should move toward Global World Up projected onto the face.
        Vector3 worldClimbUp = Vector3.up - Vector3.Project(Vector3.up, surfaceNormal);
        
        // Fallback for perfectly flat horizontal ground where World Up is parallel to the normal
        if (worldClimbUp.sqrMagnitude < 0.001f)
        {
            worldClimbUp = Vector3.forward - Vector3.Project(Vector3.forward, surfaceNormal);
        }
        worldClimbUp.Normalize();

        // Pushing 'D' (moveInput.x) should move perpendicular to both the normal and our climb direction (the horizon line)
        Vector3 worldClimbRight = Vector3.Cross(worldClimbUp, surfaceNormal).normalized;

        // 4. Generate movement delta using our newly aligned world-projection steering vectors
        Vector3 worldMoveDelta = ((worldClimbRight * -moveInput.x) + (worldClimbUp * moveInput.y)) * speed * Time.deltaTime;

        // 5. Calculate target position in World Space
        Vector3 currentWorldPos = worldA * barycentricCoords.x + worldB * barycentricCoords.y + worldC * barycentricCoords.z;
        Vector3 targetWorldPos = currentWorldPos + worldMoveDelta;

        // 6. Evaluate coordinates using the synchronized World Space calculation method
        Vector3 newBarycentric = CalculateBarycentricWorld(targetWorldPos, worldA, worldB, worldC);

        // 7. Evaluate triangle border cross checks
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
            // Crossed triangle border bounds! Shift cleanly to neighbor
            HandleTriangleTransitionWorld(targetWorldPos, newBarycentric);
        }

        UpdatePhysicalPosition();
    }

    private void HandleTriangleTransitionWorld(Vector3 targetWorldPos, Vector3 failedBarycentric)
    {
        var neighbors = scanner.triangleNeighbors[currentTriangleIndex];
        Transform meshTransform = scanner.transform;
        Matrix4x4 localToWorld = meshTransform.localToWorldMatrix;

        int bestNeighbor = -1;
        float bestDistance = float.MaxValue;

        if (neighbors != null && neighbors.Count > 0)
        {
            foreach (int neighborIndex in neighbors)
            {
                int tBase = neighborIndex * 3;
                Vector3 worldA = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase]]);
                Vector3 worldB = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase + 1]]);
                Vector3 worldC = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase + 2]]);

                Vector3 neighborBary = CalculateBarycentricWorld(targetWorldPos, worldA, worldB, worldC);
                
                float outOfBoundsPenalty = Mathf.Max(0, -neighborBary.x) + Mathf.Max(0, -neighborBary.y) + Mathf.Max(0, -neighborBary.z);
                if (outOfBoundsPenalty < bestDistance)
                {
                    bestDistance = outOfBoundsPenalty;
                    bestNeighbor = neighborIndex;
                }
            }
        }

        if (bestNeighbor != -1 && bestDistance < 0.5f)
        {
            currentTriangleIndex = bestNeighbor;
            
            int tBase = currentTriangleIndex * 3;
            Vector3 worldA = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase]]);
            Vector3 worldB = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase + 1]]);
            Vector3 worldC = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase + 2]]);

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

        int tBase = currentTriangleIndex * 3;
        Transform meshTransform = scanner.transform;
        Matrix4x4 localToWorld = meshTransform.localToWorldMatrix;

        Vector3 worldA = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase]]);
        Vector3 worldB = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase + 1]]);
        Vector3 worldC = localToWorld.MultiplyPoint3x4(vertices[triangles[tBase + 2]]);

        // 1. Reconstruct position perfectly in world space coordinates
        transform.position = worldA * barycentricCoords.x + worldB * barycentricCoords.y + worldC * barycentricCoords.z;

        // 2. Compute surface normal 
        Vector3 surfaceNormal = Vector3.Cross(worldB - worldA, worldC - worldA).normalized;

        if (surfaceNormal.sqrMagnitude > 0.001f)
        {
            // 3. Keep the visual model pointing up the face towards World Up
            Vector3 visualForward = Vector3.up - Vector3.Project(Vector3.up, surfaceNormal);

            if (visualForward.sqrMagnitude < 0.001f)
            {
                visualForward = Vector3.forward - Vector3.Project(Vector3.forward, surfaceNormal);
            }

            visualForward.Normalize();

            // 4. Update physical transform look orientation (Forward = climb heading, Up = surface normal)
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
