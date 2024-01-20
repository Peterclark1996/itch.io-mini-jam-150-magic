using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Util;

public class Spell : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer upArrow, downArrow, leftArrow, rightArrow;

    [SerializeField]
    Dictionary<int, FloorName> floors = new Dictionary<int, FloorName>();
    
    List<Key> lastFourKeysPressed = new List<Key>();

    float lockoutStart;
    bool lockedOut;

    // Update is called once per frame
    void Update()
    {
        if (!lockedOut) {
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
        else {
            upArrow.color = Color.red;
            downArrow.color = Color.red;
            leftArrow.color = Color.red;
            rightArrow.color = Color.red;
            if (Time.time - lockoutStart > 2f) {
                lockedOut = false;
                upArrow.color = Color.white;
                downArrow.color = Color.white;
                leftArrow.color = Color.white;
                rightArrow.color = Color.white;
            }
        }
        
    }

    void ValidateCombination() {
        FloorName targetFloor;
        if (floors.TryGetValue(GetKeySequenceHashCode(lastFourKeysPressed), out targetFloor)) {
            // Call code for going to floor
        }
        else {
            lockedOut = true;
            lockoutStart = Time.time;
        }
        lastFourKeysPressed = new List<Key>();
    }
}
