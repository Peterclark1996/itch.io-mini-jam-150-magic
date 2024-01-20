using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
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
        }
        if (Input.GetButtonDown("Down")) {
            lastFourKeysPressed.Add(Keys.Down);
        }
        if (Input.GetButtonDown("Left")) {
            lastFourKeysPressed.Add(Keys.Left);
        }
        if (Input.GetButtonDown("Right")) {
            lastFourKeysPressed.Add(Keys.Right);
        }
    }
}
