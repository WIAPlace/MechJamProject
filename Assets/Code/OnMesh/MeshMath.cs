using UnityEngine;


// script for calculating stuff
public static class MeshMath
{
    
    // using the new position 'f' and corner positions of the triangle that 'f' is inside of
    // 'a,b,c' to find that points barrycentric coorditate on the new frame
    public static Vector3 GetBarycentricCoordinates(Vector3 f, Vector3 a,Vector3 b,Vector3 c)
    {
        Vector3 v0 = b-a, v1 = c-a, v2 = f-a;

        float d00 = Vector3.Dot(v0,v0);
        float d01 = Vector3.Dot(v0,v1);
        float d11 = Vector3.Dot(v1,v1);
        float d20 = Vector3.Dot(v2,v0);
        float d21 = Vector3.Dot(v2,v1);

        float denom = d00 * d11 - d01 * d01;
        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;

        return new Vector3(u,v,w);
    }
}
