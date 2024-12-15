using UnityEngine;

public static class Utility
{
    public static Vector3[] ToVector3(this Vector2[] vectors)
    {
        return System.Array.ConvertAll<Vector2, Vector3>(vectors, static v => v);
    }
}