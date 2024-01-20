using UnityEngine;

public class Constants : MonoBehaviour
{
    public float liftMaxLeftPosition;
    public float liftMaxRightPosition;
    public float offScreenPosition;
    public float floorMinHeight;
    public float floorMaxHeight;
    
    public static Constants Instance;

    private void Awake()
    {
        Instance = gameObject.GetComponent<Constants>();
    }
}