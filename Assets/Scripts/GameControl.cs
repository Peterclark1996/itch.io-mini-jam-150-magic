using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameControl : MonoBehaviour
{
    public GameObject monkeyPrefab;
    public GameObject monkeyManagerPrefab;
    public GameObject scorePopupPrefab;
    public GameObject backgroundObject;

    public GamePhase currentPhase = GamePhase.INTRO;
    public FloorName currentFloor = Enum.GetValues(typeof(FloorName)).Cast<FloorName>().ToList().First();
    public int score;

    private const float TravelTimeBetweenFloors = 1.5f;
    private const float SpawnDistanceBetweenPlayerAndManager = 3.0f;
    private const float ScorePopupOffset = 3.0f;
    private const float LiftFailureJiggleOffset = 0.02f;

    private readonly List<MonkeyObject> _monkeys = new();
    private readonly List<FloorName> _allFloors = Enum.GetValues(typeof(FloorName)).Cast<FloorName>().ToList();
    private FloorName? _targetFloor;
    private float? _floorChangedTime;

    public void AwardScore(bool wasLate, Vector3 monkeyPosition)
    {
        var scoreToAdd = wasLate ? 30 : 100;

        var spawnPosition = monkeyPosition.WithY(monkeyPosition.y + ScorePopupOffset);
        var scorePopupObject = Instantiate(scorePopupPrefab, spawnPosition, new Quaternion());
        scorePopupObject.GetComponent<ScorePopupObject>().Init(scoreToAdd);

        score += scoreToAdd;
    }

    public int CountMonkeysOnLift() =>
        _monkeys.Count(monkey => monkey.transform.position.x > Constants.Instance.liftMaxLeftPosition);

    public void OnMonkeyFinishedMoving()
    {
        if (currentPhase != GamePhase.MONKEY_MOVEMENT) return;

        var areAnyMonkeysMoving = _monkeys.Any(monkey => monkey.IsMoving());
        if (areAnyMonkeysMoving) return;

        if (CountMonkeysOnLift() <= Constants.Instance.maxOccupancyLimit)
        {
            currentPhase = GamePhase.PLAYER_INPUT;
            return;
        }

        currentPhase = GamePhase.LIFT_MOVEMENT_FAILED;

        StartCoroutine(GoToPlayerFailedPhaseAfterDelay());
    }

    private IEnumerator GoToPlayerFailedPhaseAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        currentPhase = GamePhase.PLAYER_FAILED;

        foreach (var monkeyObject in _monkeys)
        {
            monkeyObject.LeaveAngrily();
        }

        SpawnManagerMonkey();
    }

    public void DestroyMonkey(MonkeyObject monkey)
    {
        _monkeys.Remove(monkey);
        Destroy(monkey.gameObject);
    }

    public void GoToGameOverScreenPhase()
    {
        currentPhase = GamePhase.GAME_OVER_SCREEN;
    }

    public void GoToMonkeyMovementPhase()
    {
        currentPhase = GamePhase.MONKEY_MOVEMENT;
        SpawnRiderMonkey();
    }

    public static GameControl Instance;

    private void Awake()
    {
        Instance = gameObject.GetComponent<GameControl>();
    }

    private void Start()
    {
        SpawnPlayerMonkey();
        SpawnManagerMonkey();
    }

    private void Update()
    {
        var roundedHeightValue = (float) (Math.Round(backgroundObject.transform.position.y / 10) * 10);
        if (currentPhase == GamePhase.LIFT_MOVEMENT_FAILED)
        {
            var newHeight = roundedHeightValue +
                            (Util.IsFrameAlternative(1, 6) ? LiftFailureJiggleOffset : -LiftFailureJiggleOffset);
            backgroundObject.transform.position = backgroundObject.transform.position.WithY(newHeight);
        }

        if (currentPhase == GamePhase.PLAYER_FAILED)
        {
            backgroundObject.transform.position = backgroundObject.transform.position.WithY(roundedHeightValue);
        }

        if (_targetFloor == null || !_floorChangedTime.HasValue || currentPhase != GamePhase.LIFT_MOVEMENT) return;

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

            GoToMonkeyMovementPhase();

            return;
        }

        var timePassed = Time.time - _floorChangedTime.Value;
        var totalTravelDuration = Math.Abs(startIndex - targetIndex) * TravelTimeBetweenFloors;
        var movementProgress = timePassed / totalTravelDuration;
        var movementProgressSmoothed = Mathf.SmoothStep(0, 1, movementProgress);

        backgroundObject.transform.position = Vector3.Lerp(startPos, targetPos, movementProgressSmoothed);
    }

    private void SpawnManagerMonkey()
    {
        var managerSpawnX = Constants.Instance.offScreenHorizontalPosition - SpawnDistanceBetweenPlayerAndManager;
        var managerSpawnPos = new Vector3(managerSpawnX, Constants.Instance.floorMinHeight, 0);
        var managerMonkey = Instantiate(monkeyManagerPrefab, managerSpawnPos, new Quaternion())
            .GetComponent<MonkeyObject>();
        managerMonkey.Init(MonkeyType.MANAGER);
        _monkeys.Add(managerMonkey);
    }

    private void SpawnPlayerMonkey()
    {
        var differenceInMaxAndMinHeight = Constants.Instance.floorMaxHeight - Constants.Instance.floorMinHeight;
        var playerSpawnPos = new Vector3(Constants.Instance.offScreenHorizontalPosition,
            Constants.Instance.floorMinHeight + differenceInMaxAndMinHeight / 2, 0);
        var playerMonkey = Instantiate(monkeyPrefab, playerSpawnPos, new Quaternion()).GetComponent<MonkeyObject>();
        playerMonkey.Init(MonkeyType.PLAYER);
        _monkeys.Add(playerMonkey);
    }

    private void SpawnRiderMonkey()
    {
        var spawnHeight = Random.Range(Constants.Instance.floorMinHeight, Constants.Instance.floorMaxHeight);
        var spawnPos = new Vector3(Constants.Instance.offScreenHorizontalPosition, spawnHeight, 0);
        var newMonkey = Instantiate(monkeyPrefab, spawnPos, new Quaternion());
        var monkeyObject = newMonkey.GetComponent<MonkeyObject>();
        _monkeys.Add(monkeyObject);

        var allValidFloors = _allFloors.Where(floor => floor != currentFloor).ToList();
        var desiredFloor = allValidFloors[Random.Range(0, allValidFloors.Count)];
        monkeyObject.Init(MonkeyType.RIDER, desiredFloor);
    }

    public void StartMovingTo(FloorName targetFloorName)
    {
        if (targetFloorName == currentFloor) return;

        currentPhase = GamePhase.LIFT_MOVEMENT;
        _targetFloor = targetFloorName;
        _floorChangedTime = Time.time;
    }
}