using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExit : MonoBehaviour
{
    public enum SceneExitType
    {
        Backward,
        Forward
    }

    [Header("Scene Change")]
    [SerializeField] private string nextSceneName;

    [Header("Spawn Point In The Next Scene")]
    [SerializeField] private string targetSpawnId;

    [Header("Exit Type")]
    [SerializeField] private SceneExitType exitType = SceneExitType.Forward;

    [Header("Transition")]
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private float transitionDelay = 1.3f;

    private bool isTransitioning;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (isTransitioning)
            return;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[SceneExit] Next scene name is empty.");
            return;
        }

        StartCoroutine(LoadSceneWithDelay());
    }

    private IEnumerator LoadSceneWithDelay()
    {
        isTransitioning = true;

        string currentLevelID = SceneManager.GetActiveScene().name;

        if (exitType == SceneExitType.Forward)
            HandleForwardExit(currentLevelID);

        SavePlayerHealthBeforeSceneChange();
        PrepareSceneTransitionData();

        if (transitionPanel != null)
        {
            transitionPanel.SetActive(true);
            yield return new WaitForSeconds(transitionDelay);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void HandleForwardExit(string currentLevelID)
    {
        // Guardamos el resumen de monedas del nivel actual.
        // Esto sirve para el resumen final del mapa.
        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.SaveCurrentLevelCoinSummary();

        bool shouldEvaluateDifficulty = ShouldEvaluateDifficulty(currentLevelID);

        if (!shouldEvaluateDifficulty)
        {
            Debug.Log($"[SceneExit] {currentLevelID} already completed. Skipping adaptive evaluation.");
            return;
        }

        // Evaluamos la dificultad solo la primera vez que se completa el nivel
        // y solo si se sale por una salida de avance.
        if (AdaptiveDifficultyManager.Instance != null)
            AdaptiveDifficultyManager.Instance.EvaluatePlayerPerformance();

        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.MarkCompleted(currentLevelID);
    }

    private bool ShouldEvaluateDifficulty(string currentLevelID)
    {
        if (LevelProgressManager.Instance == null)
            return true;

        return !LevelProgressManager.Instance.HasBeenCompleted(currentLevelID);
    }

    private void PrepareSceneTransitionData()
    {
        if (SceneTransitionManager.instance == null)
            return;

        SceneTransitionManager.instance.SetTransitionData(
            SceneManager.GetActiveScene().name,
            targetSpawnId
        );
    }

    private void SavePlayerHealthBeforeSceneChange()
    {
        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.SaveCurrentHealth();
    }
}