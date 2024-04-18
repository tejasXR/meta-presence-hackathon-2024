using UnityEngine;

public static class VectorUtil
{
    public static bool IsValid(this Vector3 vector)
    {
        return vector.x.IsValid() && vector.y.IsValid() && vector.z.IsValid();
    }

    public static bool IsValid(this float component)
    {
        return !float.IsNaN(component) && !float.IsInfinity(component);
    }
}
