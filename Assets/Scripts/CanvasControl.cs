using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasControl : MonoBehaviour
{
    public GameObject darknessObject;
    public GameObject scoreObject;
    public GameObject retryButtonObject;
    public GameObject visionMaskObject;

    private void Update()
    {
        scoreObject.GetComponent<TextMeshProUGUI>().text = $"Score: {GameControl.Instance.score}";
        visionMaskObject.SetActive(true);

        if (GameControl.Instance.currentPhase == GamePhase.GAME_OVER_SCREEN)
        {
            darknessObject.SetActive(true);
            scoreObject.SetActive(true);
            retryButtonObject.SetActive(true);
        }
        else
        {
            darknessObject.SetActive(false);
            scoreObject.SetActive(false);
            retryButtonObject.SetActive(false);
        }
    }

    public void OnTryAgainPressed() => SceneManager.LoadScene("main");
}