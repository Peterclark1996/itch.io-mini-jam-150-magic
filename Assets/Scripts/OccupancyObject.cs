using TMPro;
using UnityEngine;

public class OccupancyObject : MonoBehaviour
{
    public GameObject numberText;
    private TextMeshPro _numberTextMesh;

    private void Awake()
    {
        _numberTextMesh = numberText.GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        _numberTextMesh.text = $"{GameControl.Instance.CountMonkeysOnLift()}/{Constants.Instance.maxOccupancyLimit}";
    }
}