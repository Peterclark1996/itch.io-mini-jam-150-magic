using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameControl : MonoBehaviour
{
    public GameObject monkeyPrefab;
    public GameObject backgroundObject;

    public GamePhase currentPhase = GamePhase.MONKEY_MOVEMENT;
    public FloorName currentFloor = Enum.GetValues(typeof(FloorName)).Cast<FloorName>().ToList().First();

    private const float TravelTimeBetweenFloors = 1.5f;

    private readonly List<MonkeyObject> _monkeys = new();
    private readonly List<FloorName> _allFloors = Enum.GetValues(typeof(FloorName)).Cast<FloorName>().ToList();
    private FloorName? _targetFloor;
    private float? _floorChangedTime;

    public int CountMonkeysOnLift() => _monkeys.Count(monkey =>
    {
        var monkeyX = monkey.transform.position.x;
        return monkeyX < Constants.Instance.liftMaxRightPosition && monkeyX > Constants.Instance.liftMaxLeftPosition;
    });

    public void OnMonkeyFinishedMoving()
    {
        var areAnyMonkeysMoving = _monkeys.Any(monkey => monkey.IsMoving());
        if (areAnyMonkeysMoving) return;

        currentPhase = GamePhase.PLAYER_INPUT;
    }

    public static GameControl Instance;

    private void Awake()
    {
        Instance = gameObject.GetComponent<GameControl>();
    }

    private void Start()
    {
        SpawnMonkey();
    }

    private void Update()
    {
        // Start Temp: handles user input, goes to a random floor
        if (Input.GetButtonDown("Jump") && currentPhase == GamePhase.PLAYER_INPUT)
        {
            var allValidFloors = _allFloors.Where(floor => floor != currentFloor).ToList();
            StartMovingTo(allValidFloors[Random.Range(0, allValidFloors.Count)]);
        }
        // End Temp

        if (_targetFloor == null || !_floorChangedTime.HasValue) return;

        var startIndex = _allFloors.FindIndex(floor => floor == currentFloor);
        var targetIndex = _allFloors.FindIndex(floor => floor == _targetFloor);
        var startPos = new Vector3(0, -startIndex * 10, 0);
        var targetPos = new Vector3(0, -targetIndex * 10, 0);

        if (Vector3.Distance(backgroundObject.transform.position, targetPos) < Mathf.Epsilon)
        {
            currentFloor = _targetFloor.Value;
            _targetFloor = null;
            _floorChangedTime = null;
            
            foreach (var monkeyObject in _monkeys)
            {
                monkeyObject.OnLiftArrivedAtFloor();
            }

            currentPhase = GamePhase.MONKEY_MOVEMENT;
            SpawnMonkey();

            return;
        }

        var timePassed = Time.time - _floorChangedTime.Value;
        var totalTravelDuration = Math.Abs(startIndex - targetIndex) * TravelTimeBetweenFloors;
        var movementProgress = timePassed / totalTravelDuration;
        var movementProgressSmoothed = Mathf.SmoothStep(0, 1, movementProgress);

        backgroundObject.transform.position = Vector3.Lerp(startPos, targetPos, movementProgressSmoothed);
    }

    private void SpawnMonkey()
    {
        var spawnHeight = Random.Range(Constants.Instance.floorMinHeight, Constants.Instance.floorMaxHeight);
        var spawnPos = new Vector3(Constants.Instance.offScreenPosition, spawnHeight, 0);
        var newMonkey = Instantiate(monkeyPrefab, spawnPos, new Quaternion());
        var monkeyObject = newMonkey.GetComponent<MonkeyObject>();
        _monkeys.Add(monkeyObject);
        
        var allValidFloors = _allFloors.Where(floor => floor != currentFloor).ToList();
        var desiredFloor = allValidFloors[Random.Range(0, allValidFloors.Count)];
        monkeyObject.Init(desiredFloor);
    }

    public void StartMovingTo(FloorName targetFloorName)
    {
        if (targetFloorName == currentFloor) return;

        currentPhase = GamePhase.LIFT_MOVEMENT;
        _targetFloor = targetFloorName;
        _floorChangedTime = Time.time;
    }
}