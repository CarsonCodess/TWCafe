using UnityEngine;

public static class Extensions
{
    public static Vector3 ReplaceY(this Vector3 g, float with)
    {
        return new Vector3(g.x, with, g.z);
    }
}
