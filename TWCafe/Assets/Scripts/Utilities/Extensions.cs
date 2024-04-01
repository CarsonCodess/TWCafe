using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class Extensions
{
    public static Vector3 ReplaceY(this Vector3 g, float with)
    {
        return new Vector3(g.x, with, g.z);
    }

    public static List<int> DefaultEmptyList()
    {
        return new List<int> {0};
    }
    
    public static int[] DefaultEmptyArray()
    {
        return new [] {0};
    }
    
    public static List<T> ToList<T>(this NetworkList<T> networkList) where T : unmanaged, IEquatable<T>
    {
        var list = new List<T>();
        foreach (var item in networkList)
            list.Add(item);
        return list;
    }
}
