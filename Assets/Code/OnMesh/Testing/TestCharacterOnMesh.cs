using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class TestCharacterOnMesh : MonoBehaviour
{
    public bool logTriangles;
    public float rayDistance = 10f;
    public LayerMask indexMask;
    

    private Collider previousHitCollider = null;
    private MeshEdgeScanner scanner;
    private int debugLastTriangleIndex = -1;


    
    [Header("Debug/Editor Stuff")]
    public Vector3 baryCoords;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        DrawLineForward();
        // Example: Cast a ray straight forward from this object
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, rayDistance ,indexMask))
        {
            if(previousHitCollider == null || hit.collider != previousHitCollider)
            {    // Verify we hit a mesh scanner target 
                scanner = hit.collider.GetComponent<MeshEdgeScanner>();
                previousHitCollider = hit.collider;

                // only send this to console if it is wanted
                if(logTriangles) Debug.Log(hit.collider.name);
            }

            if (scanner != null)
            {
                // Capture the exact hit triangle index
                int hitTriangleIndex = hit.triangleIndex;

                // -1 indicates a non-mesh collider or invalid hit
                if (hitTriangleIndex != -1)
                {
                    if(hitTriangleIndex != debugLastTriangleIndex && logTriangles){
                        Debug.Log($"Raycast hit Triangle ID: {hitTriangleIndex}");
                        debugLastTriangleIndex = hitTriangleIndex;
                    }
                    DrawDebugTriangleEdges(hitTriangleIndex,scanner);

                    baryCoords = hit.barycentricCoordinate;
                    DebugShowBaryCoords(hit, scanner);
                }
            }
        }
    }

    void DrawDebugTriangleEdges(int hitTriIndex, MeshEdgeScanner scanner)
    {
        Mesh mesh = scanner.GetComponent<MeshFilter>().mesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Transform meshTransform = scanner.transform;

        // 1. FIRST: Draw the main hit triangle in solid red
        int hitStart = hitTriIndex * 3;
        Vector3 hitA = meshTransform.TransformPoint(vertices[triangles[hitStart]]);
        Vector3 hitB = meshTransform.TransformPoint(vertices[triangles[hitStart + 1]]);
        Vector3 hitC = meshTransform.TransformPoint(vertices[triangles[hitStart + 2]]);

        // Define a small surface offset to prevent the lines from sinking into the mesh geometry
        Vector3 hitNormal = Vector3.Cross(hitB - hitA, hitC - hitA).normalized * 0.005f;

        Debug.DrawLine(hitA + hitNormal, hitB + hitNormal, Color.red);
        Debug.DrawLine(hitB + hitNormal, hitC + hitNormal, Color.red);
        Debug.DrawLine(hitC + hitNormal, hitA + hitNormal, Color.red);

        // Create a quick lookup of the main triangle's vertex indices
        HashSet<int> mainTriangleVertices = new HashSet<int> 
        { 
            triangles[hitStart], 
            triangles[hitStart + 1], 
            triangles[hitStart + 2] 
        };

        // 2. SECOND: Draw only the outer, unshared edges of the neighbor triangles in yellow
        if (hitTriIndex < scanner.triangleNeighbors.Length)
        {
            foreach (int neighborIndex in scanner.triangleNeighbors[hitTriIndex])
            {
                int nStart = neighborIndex * 3;
                int v0 = triangles[nStart];
                int v1 = triangles[nStart + 1];
                int v2 = triangles[nStart + 2];

                Vector3 nA = meshTransform.TransformPoint(vertices[v0]);
                Vector3 nB = meshTransform.TransformPoint(vertices[v1]);
                Vector3 nC = meshTransform.TransformPoint(vertices[v2]);
                
                Vector3 nNormal = Vector3.Cross(nB - nA, nC - nA).normalized * 0.004f;

                // Only draw the edge if BOTH vertices are not part of the main red triangle
                if (!(mainTriangleVertices.Contains(v0) && mainTriangleVertices.Contains(v1)))
                    Debug.DrawLine(nA + nNormal, nB + nNormal, Color.yellow);

                if (!(mainTriangleVertices.Contains(v1) && mainTriangleVertices.Contains(v2)))
                    Debug.DrawLine(nB + nNormal, nC + nNormal, Color.yellow);

                if (!(mainTriangleVertices.Contains(v2) && mainTriangleVertices.Contains(v0)))
                    Debug.DrawLine(nC + nNormal, nA + nNormal, Color.yellow);
            }
        }
    }

    private void DrawLineForward()
    {
        // Define the start and end points
        Vector3 startPoint = transform.position;
        Vector3 endPoint = startPoint + (-transform.up* rayDistance);

        // Draw the line in the Scene View (Color, Duration)
        Debug.DrawLine(startPoint, endPoint, Color.green);
    }
    
    private void DebugShowBaryCoords(RaycastHit hit, MeshEdgeScanner scanner)
    {
        Mesh mesh = scanner.GetComponent<MeshFilter>().mesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Transform meshTransform = scanner.transform;

        // Extract the 3 local vertex positions of the hit triangle
        Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

        // Calculate the local position using the Barycentric Weights (x=u, y=v, z=w)
        Vector3 baryB = hit.barycentricCoordinate;
        Vector3 localHitPoint = p0 * baryB.x + p1 * baryB.y + p2 * baryB.z;

        // Transform the local point into 3D World Space coordinates
        Vector3 worldHitPoint = hit.transform.TransformPoint(localHitPoint);

        // draw a cross hair at the point
        Debug.DrawLine(worldHitPoint + Vector3.up * 0.2f, worldHitPoint + Vector3.down * 0.2f, Color.green);
        Debug.DrawLine(worldHitPoint + Vector3.left * 0.2f, worldHitPoint + Vector3.right * 0.2f, Color.green);
        Debug.DrawLine(worldHitPoint + Vector3.forward * 0.2f, worldHitPoint + Vector3.back * 0.2f, Color.green);

    }
}
