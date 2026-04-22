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

    [Header("Scene change")]
    [SerializeField] private string nextSceneName;

    [Header("Spawn point in the next scene")]
    [SerializeField] private string targetSpawnId;

    [Header("Exit type")]
    [SerializeField] private SceneExitType exitType = SceneExitType.Forward;

    [Header("Transition")]
    [SerializeField] private GameObject transitionPanel;

    private bool isTransitioning;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (isTransitioning)
            return;

        StartCoroutine(LoadSceneWithDelay());
    }

    private IEnumerator LoadSceneWithDelay()
    {
        isTransitioning = true;

        string currentLevelID = SceneManager.GetActiveScene().name;

        if (exitType == SceneExitType.Forward)
        {
            bool shouldEvaluate = true;

            if (LevelProgressManager.Instance != null)
                shouldEvaluate = !LevelProgressManager.Instance.HasBeenCompleted(currentLevelID);

            if (shouldEvaluate)
            {
                if (AdaptiveDifficultyManager.Instance != null)
                {
                    Debug.Log("Evalua");
                    AdaptiveDifficultyManager.Instance.EvaluatePlayerPerformance();
                }

                if (LevelProgressManager.Instance != null)
                    LevelProgressManager.Instance.MarkCompleted(currentLevelID);
            }
        }

        if (transitionPanel != null)
            transitionPanel.SetActive(true);

        if (SceneTransitionManager.instance != null)
        {
            SceneTransitionManager.instance.SetTransitionData(
                SceneManager.GetActiveScene().name,
                targetSpawnId
            );
        }

        if (transitionPanel != null)
            yield return new WaitForSeconds(1.30f);

        SceneManager.LoadScene(nextSceneName);
    }
}