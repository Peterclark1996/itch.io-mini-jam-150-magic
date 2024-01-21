using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Util
{
    public static Vector3 WithY(this Vector3 originalVector, float newValue) =>
        new(originalVector.x, newValue, originalVector.z);
    
    public static Vector3 WithX(this Vector3 originalVector, float newValue) =>
        new(newValue, originalVector.y, originalVector.z);

    public enum Key {
        Up,
        Down,
        Left,
        Right
    }

    public static int GetKeySequenceHashCode(List<Key> keys) {
        return keys.Aggregate(487, (current, item) =>
            (current * 31) + item.GetHashCode());
    }
    
    public static bool RandomBool() => Random.Range(0, 2) == 0;
}