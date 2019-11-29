using System;
using UnityEngine;

public static class MathTools
{
    public static int Sign(float value)
    {
        return (value > 0 ? 1 : 0) - (value < 0 ? 1 : 0);
    }

    public static Vector3 CrossProduct(Vector3 a, Vector3 b)
    {
        return new Vector3((a.y * b.z) - (a.z * b.y), (a.z * b.x) - (a.x * b.z), (a.x * b.y) - (a.y * b.x));
    }
}
