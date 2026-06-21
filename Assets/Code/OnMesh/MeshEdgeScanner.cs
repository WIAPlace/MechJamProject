using UnityEngine;
using System.Collections.Generic;

public class MeshEdgeScanner : MonoBehaviour
{
    public struct Edge
    {
        public Vector3 pos1;
        public Vector3 pos2;

        public Edge(Vector3 p1, Vector3 p2)
        {
            // Vector comparison logic to sort points predictably
            if (p1.x < p2.x || (p1.x == p2.x && p1.y < p2.y) || (p1.x == p2.x && p1.y == p2.y && p1.z < p2.z))
            {
                pos1 = p1; pos2 = p2;
            }
            else
            {
                pos1 = p2; pos2 = p1;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge)) return false;
            Edge other = (Edge)obj;
            
            // Tiny math threshold (epsilon) to avoid minor rounding float issues
            return Vector3.Distance(pos1, other.pos1) < 0.001f && 
                   Vector3.Distance(pos2, other.pos2) < 0.001f;
        }

        public override int GetHashCode()
        {
            // Quantize floating points into rough integer steps
            int x1 = Mathf.RoundToInt(pos1.x * 1000f);
            int y1 = Mathf.RoundToInt(pos1.y * 1000f);
            int z1 = Mathf.RoundToInt(pos1.z * 1000f);
            int x2 = Mathf.RoundToInt(pos2.x * 1000f);
            int y2 = Mathf.RoundToInt(pos2.y * 1000f);
            int z2 = Mathf.RoundToInt(pos2.z * 1000f);

            // Use a proper multiplier to avoid XOR collisions
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + x1;
                hash = hash * 23 + y1;
                hash = hash * 23 + z1;
                hash = hash * 23 + x2;
                hash = hash * 23 + y2;
                hash = hash * 23 + z2;
                return hash;
            }
        }
    }

    public List<int>[] triangleNeighbors;
    public Dictionary<Edge, List<int>> edgeToTriangles = new Dictionary<Edge, List<int>>();

    void Awake()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = meshFilter.mesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        edgeToTriangles = new Dictionary<Edge, List<int>>();

        // STEP 1: Scan all triangles and map edges via physical coordinates
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int triangleIndex = i / 3;

            Vector3 posA = vertices[triangles[i]];
            Vector3 posB = vertices[triangles[i + 1]];
            Vector3 posC = vertices[triangles[i + 2]];

            Edge[] edges = new Edge[]
            {
                new Edge(posA, posB),
                new Edge(posB, posC),
                new Edge(posC, posA)
            };

            foreach (Edge edge in edges)
            {
                if (!edgeToTriangles.ContainsKey(edge))
                    edgeToTriangles[edge] = new List<int>();

                edgeToTriangles[edge].Add(triangleIndex);
            }
        }

        // STEP 2: Create triangle neighbor connections
        int triangleCount = triangles.Length / 3;
        triangleNeighbors = new List<int>[triangleCount];

        for (int i = 0; i < triangleNeighbors.Length; i++)
        {
            triangleNeighbors[i] = new List<int>();
        }

        foreach (KeyValuePair<Edge, List<int>> kvp in edgeToTriangles)
        {
            List<int> connectedTris = kvp.Value;

            if (connectedTris.Count > 1)
            {
                for (int a = 0; a < connectedTris.Count; a++)
                {
                    for (int b = a + 1; b < connectedTris.Count; b++)
                    {
                        int triA = connectedTris[a];
                        int triB = connectedTris[b];

                        // FIXED: Correctly assign two-way relationships
                        if (!triangleNeighbors[triA].Contains(triB)) triangleNeighbors[triA].Add(triB);
                        if (!triangleNeighbors[triB].Contains(triA)) triangleNeighbors[triB].Add(triA);
                    }
                }
            }
        }
    }
}
