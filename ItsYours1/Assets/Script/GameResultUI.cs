
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResultUI : MonoBehaviour
{
    [Header("Result Panels")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    private bool resultShown = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (defeatPanel != null)
            defeatPanel.SetActive(false);
    }

    public void ShowVictory()
    {
        if (resultShown) return;

        resultShown = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ShowDefeat()
    {
        if (resultShown) return;

        resultShown = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (defeatPanel != null)
            defeatPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
