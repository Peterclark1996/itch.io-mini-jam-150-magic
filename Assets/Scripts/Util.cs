using UnityEngine;

public static class Util
{
    public static Vector3 WithY(this Vector3 originalVector, float newValue) =>
        new(originalVector.x, newValue, originalVector.z);
    
    public static Vector3 WithX(this Vector3 originalVector, float newValue) =>
        new(newValue, originalVector.y, originalVector.z);
}