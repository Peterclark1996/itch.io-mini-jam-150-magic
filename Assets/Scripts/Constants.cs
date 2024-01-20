using UnityEngine;

public class Constants : MonoBehaviour
{
    public float LiftMaxLeftPosition;
    public float LiftMaxRightPosition;
    public float OffScreenPosition;
    public float FloorHeight;
    
    public static Constants Instance;

    private void Awake()
    {
        Instance = gameObject.GetComponent<Constants>();
    }
}