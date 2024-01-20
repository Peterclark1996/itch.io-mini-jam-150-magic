using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameControl : MonoBehaviour
{
    public GameObject monkeyPrefab;

    void Start()
    {
        SpawnMonkey();
    }

    private void SpawnMonkey()
    {
        var spawnPos = new Vector3(Constants.Instance.OffScreenPosition, Constants.Instance.FloorHeight, 0);
        var newMonkey = Instantiate(monkeyPrefab, spawnPos, new Quaternion());
        var monkeyObject = newMonkey.GetComponent<MonkeyObject>();
        var allFloors = Enum.GetValues(typeof(Floor));
        var desiredFloor = (Floor) allFloors.GetValue(Random.Range(0, allFloors.Length));
        monkeyObject.Init(desiredFloor);
    }
}