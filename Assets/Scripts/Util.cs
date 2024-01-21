using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Util
{
    public static Vector3 WithY(this Vector3 originalVector, float newValue) =>
        new(originalVector.x, newValue, originalVector.z);

    public static Vector3 WithX(this Vector3 originalVector, float newValue) =>
        new(newValue, originalVector.y, originalVector.z);

    public enum Key
    {
        Up,
        Down,
        Left,
        Right
    }

    public static int GetKeySequenceHashCode(List<Key> keys)
    {
        return keys.Aggregate(487, (current, item) =>
            (current * 31) + item.GetHashCode());
    }

    public static bool RandomBool() => Random.Range(0, 2) == 0;

    public static bool IsFrameAlternative(float spawnTime, int animationSpeed)
    {
        var aliveTime = Time.time - spawnTime;
        return aliveTime * animationSpeed % 2 < 1;
    }

    public static bool IsAnyKeyPressed() => Input.GetButtonDown("Up") || Input.GetButtonDown("Down") ||
                                            Input.GetButtonUp("Left") || Input.GetButtonDown("Right") ||
                                            Input.GetButtonDown("Jump");
}