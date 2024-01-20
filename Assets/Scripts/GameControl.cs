using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameControl : MonoBehaviour
{
    public GameObject monkeyPrefab;

    private readonly List<MonkeyObject> _monkeys = new();

    public int CountMonkeysOnLift() => _monkeys.Count(monkey =>
    {
        var monkeyX = monkey.transform.position.x;
        return monkeyX < Constants.Instance.LiftMaxRightPosition && monkeyX > Constants.Instance.LiftMaxLeftPosition;
    });

    public bool AreMonkeysMoving() => _monkeys.Any(monkey => monkey.isMoving());

    public static GameControl Instance;

    private void Awake()
    {
        Instance = gameObject.GetComponent<GameControl>();
    }
    
    private void Start()
    {
        SpawnMonkey();
    }

    private void SpawnMonkey()
    {
        var spawnPos = new Vector3(Constants.Instance.OffScreenPosition, Constants.Instance.FloorHeight, 0);
        var newMonkey = Instantiate(monkeyPrefab, spawnPos, new Quaternion());
        var monkeyObject = newMonkey.GetComponent<MonkeyObject>();
        _monkeys.Add(monkeyObject);
        var allFloors = Enum.GetValues(typeof(Floor));
        var desiredFloor = (Floor) allFloors.GetValue(Random.Range(0, allFloors.Length));
        monkeyObject.Init(desiredFloor);
    }
}