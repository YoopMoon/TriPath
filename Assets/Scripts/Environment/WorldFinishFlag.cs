using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldFinishFlag : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip finishClip;
    [SerializeField] private float finishVolume = 1f;

    [Header("Transition")]
    [SerializeField] private GameObject transitionObject;
    [SerializeField] private float delayBeforeTransition = 2f;
    [SerializeField] private float transitionDuration = 1.5f;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    [Header("Final Ranking")]
    [SerializeField] private bool saveRankingOnFinish;
    [SerializeField] private string finalLevelSceneName = "Map2_Level5";

    private bool isActivated;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (transitionObject != null)
            transitionObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActivated)
            return;

        if (!collision.CompareTag("Player"))
            return;

        isActivated = true;

        StartCoroutine(FinishWorldSequence());
    }

    private IEnumerator FinishWorldSequence()
    {
        if (animator != null)
            animator.enabled = true;

        PlayFinishSFX();

        yield return new WaitForSeconds(delayBeforeTransition);

        if (transitionObject != null)
            transitionObject.SetActive(true);

        yield return new WaitForSeconds(transitionDuration);

        string currentLevelID = SceneManager.GetActiveScene().name;

        SaveCurrentLevelResults();
        EvaluateAndMarkCurrentLevel(currentLevelID);

        if (saveRankingOnFinish)
            SaveFinalRankingEntry();

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private void SaveCurrentLevelResults()
    {
        if (LevelMetrics.Instance == null)
            return;

        LevelMetrics.Instance.SaveCurrentLevelCoinSummary();
        LevelMetrics.Instance.SaveCurrentLevelPerformanceSummary();

        LevelMetrics.Instance.LogLevelCoinSummaries();
        LevelMetrics.Instance.LogCurrentMapCoinSummary();
    }

    private void EvaluateAndMarkCurrentLevel(string currentLevelID)
    {
        if (!ShouldEvaluateDifficulty(currentLevelID))
        {
            Debug.Log($"[WorldFinishFlag] {currentLevelID} already completed. Skipping adaptive evaluation.");
            return;
        }

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

    private void SaveFinalRankingEntry()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName != finalLevelSceneName)
        {
            Debug.LogWarning($"[WorldFinishFlag] Ranking was requested from {currentSceneName}, but final level is {finalLevelSceneName}.");
            return;
        }

        if (LevelMetrics.Instance == null)
            return;

        string playerName = "Player";

        if (PlayerRunData.Instance != null)
            playerName = PlayerRunData.Instance.PlayerName;

        string characterName = SelectedPlayerStore.SelectedPlayer.ToString();

        int totalCoinsCollected = LevelMetrics.Instance.GetTotalCoinsCollected();
        int totalCoinsAvailable = LevelMetrics.Instance.GetTotalCoinsAvailable();
        int totalDamageTaken = LevelMetrics.Instance.GetTotalDamageTaken();
        float totalTime = LevelMetrics.Instance.GetTotalElapsedTime();

        int finalScore = CalculateFinalScore(
            totalCoinsCollected,
            totalDamageTaken,
            totalTime
        );

        RankingEntry entry = new RankingEntry
        {
            playerName = playerName,
            characterName = characterName,
            totalCoinsCollected = totalCoinsCollected,
            totalCoinsAvailable = totalCoinsAvailable,
            totalDamageTaken = totalDamageTaken,
            totalTime = totalTime,
            finalScore = finalScore
        };

        RankingStorage.SaveOrReplaceEntry(entry);

        Debug.Log($"[WorldFinishFlag] Final ranking saved for {playerName}. Score: {finalScore}");
    }

    private int CalculateFinalScore(int coinsCollected, int damageTaken, float totalTime)
    {
        int score = 0;

        score += coinsCollected * 100;
        score -= damageTaken * 50;
        score -= Mathf.RoundToInt(totalTime * 2f);

        return Mathf.Max(0, score);
    }

    private void PlayFinishSFX()
    {
        if (finishClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(finishClip, finishVolume);
    }
}