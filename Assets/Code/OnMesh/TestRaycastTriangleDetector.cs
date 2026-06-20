using UnityEngine;
using System.Collections.Generic;

public class TestRaycastTriangleDetector : MonoBehaviour
{
    public float rayDistance = 10f;

    void Update()
    {
        // Example: Cast a ray straight forward from this object
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // Verify we hit a mesh scanner target
            MeshEdgeScanner scanner = hit.collider.GetComponent<MeshEdgeScanner>();
            if (scanner != null)
            {
                // Capture the exact hit triangle index
                int hitTriangleIndex = hit.triangleIndex;

                // -1 indicates a non-mesh collider or invalid hit
                if (hitTriangleIndex != -1)
                {
                    Debug.Log($"Raycast hit Triangle ID: {hitTriangleIndex}");
                    DrawDebugTriangleEdges(hitTriangleIndex,scanner);

                    // Retrieve the neighbors using your scanner data structure
                    // (Ensure triangleNeighbors array in MeshEdgeScanner is public or accessible)
                    if (hitTriangleIndex < scanner.triangleNeighbors.Length)
                    {
                        var neighbors = scanner.triangleNeighbors[hitTriangleIndex];
                        Debug.Log($"Connected neighbor triangles: {string.Join(", ", neighbors)}");
                        //DrawDebugTriangleEdges(neighbors[hitTriangleIndex],scanner);
                    }
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
}
