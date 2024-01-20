using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer upArrow, downArrow, leftArrow, rightArrow;

    enum Keys {
        Up,
        Down,
        Left,
        Right
    }
    
    List<Keys> lastFourKeysPressed = new List<Keys>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Up")) {
            lastFourKeysPressed.Add(Keys.Up);
            upArrow.color = Color.green;
        }
        if (Input.GetButtonUp("Up")) {
            upArrow.color = Color.white;
        }
        if (Input.GetButtonDown("Down")) {
            lastFourKeysPressed.Add(Keys.Down);
            downArrow.color = Color.green;
        }
        if (Input.GetButtonUp("Down")) {
            downArrow.color = Color.white;
        }
        if (Input.GetButtonDown("Left")) {
            lastFourKeysPressed.Add(Keys.Left);
            leftArrow.color = Color.green;
        }
        if (Input.GetButtonUp("Left")) {
            leftArrow.color = Color.white;
        }
        if (Input.GetButtonDown("Right")) {
            lastFourKeysPressed.Add(Keys.Right);
            rightArrow.color = Color.green;
        }
        if (Input.GetButtonUp("Right")) {
            rightArrow.color = Color.white;
        }
    }
}
