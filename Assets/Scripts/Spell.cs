using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

using static Util;

public class Spell : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer upArrow, downArrow, leftArrow, rightArrow;

    [SerializeField]
    Transform arrowPrefab, libaryRed, libraryBlue, libraryGreen, alchemyRed, alchemyBlue, alchemyGreen;

    [SerializeField]
    GameControl gameControl;
    
    List<KeyCombination> keyCombinations = new List<KeyCombination>();
    Dictionary<int, FloorName> floors = new Dictionary<int, FloorName>();
    List<Key> lastFourKeysPressed = new List<Key>();

    float lockoutStart;
    bool lockedOut;

    void Awake()
    {
        List<FloorName> allFloors = Enum.GetValues(typeof(FloorName)).Cast<FloorName>().ToList();
        float floorOffset = 1.5f;
        Vector3 scale = new Vector3(0.25f, 0.25f, 1f);
        foreach (var floor in allFloors) {
            var combination = new List<Key>() {
                (Key)Random.Range(0, 4),
                (Key)Random.Range(0, 4),
                (Key)Random.Range(0, 4),
                (Key)Random.Range(0, 4)
            };
            // to make sure we don't get two floors with the same code
            while (!floors.TryAdd(GetKeySequenceHashCode(combination), floor)) {
                combination = new List<Key>() {
                    (Key)Random.Range(0, 4),
                    (Key)Random.Range(0, 4),
                    (Key)Random.Range(0, 4),
                    (Key)Random.Range(0, 4)
                };
            }

            KeyCombination keyCombination = new KeyCombination(
                floor,
                combination[0],
                combination[1],
                combination[2],
                combination[3]);
            keyCombinations.Add(keyCombination);

            Transform label = Instantiate(FloorSprite(floor));
            label.SetParent(transform, false);
            label.localPosition = new Vector3(-0.8f, floorOffset, 0);
            Transform arrow = Instantiate(arrowPrefab);
            arrow.SetParent(transform, false);
            arrow.localScale = scale;
            arrow.localPosition = new Vector3(0f, floorOffset, 0);
            RotateArrow(keyCombination.Key1, arrow);
            arrow = Instantiate(arrowPrefab);
            arrow.SetParent(transform, false);
            arrow.localScale = scale;
            arrow.localPosition = new Vector3(0.8f, floorOffset, 0);
            RotateArrow(keyCombination.Key2, arrow);
            arrow = Instantiate(arrowPrefab);
            arrow.SetParent(transform, false);
            arrow.localScale = scale;
            arrow.localPosition = new Vector3(1.6f, floorOffset, 0);
            RotateArrow(keyCombination.Key3, arrow);
            arrow = Instantiate(arrowPrefab);
            arrow.SetParent(transform, false);
            arrow.localScale = scale;
            arrow.localPosition = new Vector3(2.4f, floorOffset, 0);
            RotateArrow(keyCombination.Key4, arrow);

            floorOffset++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!lockedOut && gameControl.currentPhase == GamePhase.PLAYER_INPUT) {
            if (upArrow.color == Color.grey) {
                upArrow.color = Color.white;
                downArrow.color = Color.white;
                leftArrow.color = Color.white;
                rightArrow.color = Color.white;
            }
            if (Input.GetButtonDown("Up")) {
                lastFourKeysPressed.Add(Key.Up);
                upArrow.color = Color.green;
                if (lastFourKeysPressed.Count == 4)
                    ValidateCombination();
            }
            if (Input.GetButtonUp("Up")) {
                upArrow.color = Color.white;
            }
            if (Input.GetButtonDown("Down")) {
                lastFourKeysPressed.Add(Key.Down);
                downArrow.color = Color.green;
                if (lastFourKeysPressed.Count == 4)
                    ValidateCombination();
            }
            if (Input.GetButtonUp("Down")) {
                downArrow.color = Color.white;
            }
            if (Input.GetButtonDown("Left")) {
                lastFourKeysPressed.Add(Key.Left);
                leftArrow.color = Color.green;
                if (lastFourKeysPressed.Count == 4)
                    ValidateCombination();
            }
            if (Input.GetButtonUp("Left")) {
                leftArrow.color = Color.white;
            }
            if (Input.GetButtonDown("Right")) {
                lastFourKeysPressed.Add(Key.Right);
                rightArrow.color = Color.green;
                if (lastFourKeysPressed.Count == 4)
                    ValidateCombination();
            }
            if (Input.GetButtonUp("Right")) {
                rightArrow.color = Color.white;
            }
        }
        else if (lockedOut) {
            upArrow.color = Color.red;
            downArrow.color = Color.red;
            leftArrow.color = Color.red;
            rightArrow.color = Color.red;
            if (Time.time - lockoutStart > 0.5f) {
                lockedOut = false;
                upArrow.color = Color.white;
                downArrow.color = Color.white;
                leftArrow.color = Color.white;
                rightArrow.color = Color.white;
            }
        }
        else {
            upArrow.color = Color.grey;
            downArrow.color = Color.grey;
            leftArrow.color = Color.grey;
            rightArrow.color = Color.grey;
        }
        
    }

    void RotateArrow(Key key, Transform arrow) {
        switch (key) {
            case Key.Up:
                break;
            case Key.Down:
                arrow.localEulerAngles = new Vector3(0, 0, 180);
                break;
            case Key.Left:
                arrow.localEulerAngles = new Vector3(0, 0, 90);
                break;
            case Key.Right:
                arrow.localEulerAngles = new Vector3(0, 0, 270);
                break;
        }
    }

    Transform FloorSprite(FloorName floorName) {
        switch (floorName) {
            case FloorName.LIBRARY_RED:
                return libaryRed;
            case FloorName.LIBRARY_BLUE:
                return libraryBlue;
            case FloorName.LIBRARY_GREEN:
                return libraryGreen;
            case FloorName.ALCHEMY_RED:
                return alchemyRed;
            case FloorName.ALCHEMY_BLUE:
                return alchemyBlue;
            case FloorName.ALCHEMY_GREEN:
                return alchemyGreen;
            default:
                return libaryRed;
        }
    }

    void ValidateCombination() {
        FloorName targetFloor;
        if (floors.TryGetValue(GetKeySequenceHashCode(lastFourKeysPressed), out targetFloor)) {
            gameControl.StartMovingTo(targetFloor);
        }
        else {
            lockedOut = true;
            lockoutStart = Time.time;
            gameControl.GoToMonkeyMovementPhase();
        }
        lastFourKeysPressed = new List<Key>();
    }
}
