using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExit : MonoBehaviour
{
    [Header("Scene change")]
    public string nextSceneName;

    [Header("Spawn point in the next scene")]
    public string targetSpawnId;

    public GameObject transitionPanel;

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

        if (transitionPanel != null)
            transitionPanel.SetActive(true);

        SceneTransitionManager.instance.SetTransitionData(
            SceneManager.GetActiveScene().name,
            targetSpawnId
        );

        if (transitionPanel != null)
            yield return new WaitForSeconds(1.30f);

        SceneManager.LoadScene(nextSceneName);
    }
}