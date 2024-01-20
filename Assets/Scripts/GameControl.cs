using UnityEngine;

public class GameControl : MonoBehaviour
{
    public GameObject MonkeyPrefab;
    
    void Start()
    {
        SpawnMonkey();
    }

    private void SpawnMonkey()
    {
        Instantiate(MonkeyPrefab, new Vector3(Constants.Instance.OffScreenPosition, Constants.Instance.FloorHeight, 0), new Quaternion());
    }
}
