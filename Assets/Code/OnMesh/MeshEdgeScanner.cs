using UnityEngine;
using System.Collections.Generic;

public class MeshEdgeScanner : MonoBehaviour
{
    // Define a struct to represent an undirected edge
    public struct Edge
    {
        public int v1;
        public int v2;

        public Edge(int vertex1, int vertex2)
        {
            // Sort to ensure undirected edges (Edge A-B and B-A are the same)
            if (vertex1 < vertex2) { v1 = vertex1; v2 = vertex2; }
            else { v1 = vertex2; v2 = vertex1; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge)) return false;
            Edge other = (Edge)obj;
            return v1 == other.v1 && v2 == other.v2;
        }

        public override int GetHashCode()
        {
            return v1.GetHashCode() ^ v2.GetHashCode();
        }
    }
    
    public List<int>[] triangleNeighbors;

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = meshFilter.mesh;
        int[] triangles = mesh.triangles;

        // Map an edge to the list of triangles that share it
        Dictionary<Edge, List<int>> edgeToTriangles = new Dictionary<Edge, List<int>>();

        // STEP 1: Scan all triangles and map their edges
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int triangleIndex = i / 3;

            int vertA = triangles[i];
            int vertB = triangles[i + 1];
            int vertC = triangles[i + 2];

            Edge[] edges = new Edge[]
            {
                new Edge(vertA, vertB),
                new Edge(vertB, vertC),
                new Edge(vertC, vertA)
            };

            foreach (Edge edge in edges)
            {
                if (!edgeToTriangles.ContainsKey(edge))
                    edgeToTriangles[edge] = new List<int>();

                edgeToTriangles[edge].Add(triangleIndex);
            }
        }

        // STEP 2: Create triangle neighbor connections
        // Note: You can size this as `new List<int>[triangleCount]`
        int triangleCount = triangles.Length / 3;
        triangleNeighbors = new List<int>[triangleCount];

        for (int i = 0; i < triangleNeighbors.Length; i++)
        {
            triangleNeighbors[i] = new List<int>();
        }

        // Iterate through all edges to see which triangles connect
        foreach (KeyValuePair<Edge, List<int>> kvp in edgeToTriangles)
        {
            List<int> connectedTris = kvp.Value;

            // If an edge is shared by 2+ triangles, those triangles are connected
            if (connectedTris.Count > 1)
            {
                int triA = connectedTris[0];
                int triB = connectedTris[1];

                if (!triangleNeighbors[triA].Contains(triB)) triangleNeighbors[triA].Add(triB);
                if (!triangleNeighbors[triB].Contains(triA)) triangleNeighbors[triB].Add(triA);
            }
        }

        // Debug output to verify connections
        /*
        for (int i = 0; i < triangleNeighbors.Length; i++)
        {
            Debug.Log($"Triangle {i} is connected to triangles: {string.Join(", ", triangleNeighbors[i])}");
        }
        */
        // Debugging to see normals
        DebugShowNormals(mesh);
        
    }

    private void DebugShowNormals(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        float normalLength = 0.5f;
        
        Color normalColor = Color.green;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldVertex = transform.TransformPoint(vertices[i]);
            Vector3 worldNormal = transform.TransformDirection(normals[i]);

            // CHANGED: Setting depthTest (the 5th parameter) to true.
            // This forces Unity to use the depth buffer, naturally hiding 
            // lines that sit behind solid objects.
            Debug.DrawRay(worldVertex, worldNormal * normalLength, normalColor, float.MaxValue, true);
        }
    }
}
