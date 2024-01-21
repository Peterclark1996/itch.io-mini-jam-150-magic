using System;
using UnityEngine;

public class CanvasControl : MonoBehaviour
{
    public GameObject DarknessObject;
    public GameObject ScoreObject;
    public GameObject RetryButtonObject;
    public GameObject VisionMaskObject;

    private void Update()
    {
        if (GameControl.Instance.currentPhase == GamePhase.GAME_OVER_SCREEN)
        {
            DarknessObject.SetActive(true);
            ScoreObject.SetActive(true);
            RetryButtonObject.SetActive(true);
            VisionMaskObject.SetActive(false);
        }
        else
        {
            DarknessObject.SetActive(false);
            ScoreObject.SetActive(false);
            RetryButtonObject.SetActive(false);
            VisionMaskObject.SetActive(true);
        }
    }
}
