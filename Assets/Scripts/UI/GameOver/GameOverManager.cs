using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button tryAgainButton;

    [Header("Pause")]
    [SerializeField] private GameObject pauseButton;

    [Header("Settings")]
    [SerializeField] private float tryAgainDelay = 1f;

    private bool isGameOver;
    private string restartSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (tryAgainButton != null)
        {
            tryAgainButton.gameObject.SetActive(false);
            tryAgainButton.onClick.RemoveListener(TryAgain);
            tryAgainButton.onClick.AddListener(TryAgain);
        }
    }

    public void HandlePlayerDeath()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        restartSceneName = GetFirstLevelOfCurrentMap();

        DisablePauseMenu();
        StartCoroutine(GameOverRoutine());
    }

    private void DisablePauseMenu()
    {
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    private IEnumerator GameOverRoutine()
    {
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (tryAgainButton != null)
            tryAgainButton.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(tryAgainDelay);

        if (tryAgainButton != null)
            tryAgainButton.gameObject.SetActive(true);
    }

    public void TryAgain()
    {
        if (!isGameOver)
            return;

        ResetCurrentRunForCurrentMap();

        Time.timeScale = 1f;
        SceneManager.LoadScene(restartSceneName);
    }

    private void ResetCurrentRunForCurrentMap()
    {
        string mapPrefix = GetCurrentMapPrefix();

        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.ResetProgress();

        if (AdaptiveDifficultyManager.Instance != null)
            AdaptiveDifficultyManager.Instance.ResetProgress();

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.ResetMetrics();

        if (CollectibleStateManager.Instance != null)
            CollectibleStateManager.Instance.ResetProgressByMap(mapPrefix);

        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.ResetProgressByMap(mapPrefix);
    }

    private string GetFirstLevelOfCurrentMap()
    {
        string mapPrefix = GetCurrentMapPrefix();
        return mapPrefix + "_Level1";
    }

    private string GetCurrentMapPrefix()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int separatorIndex = currentSceneName.IndexOf('_');

        if (separatorIndex < 0)
            return currentSceneName;

        return currentSceneName.Substring(0, separatorIndex);
    }
}