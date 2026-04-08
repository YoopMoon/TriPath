using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExit : MonoBehaviour
{
    [Header("Scene change")]
    public string nextSceneName;

    [Header("Spawn point in the next scene")]
    public string targetSpawnId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        SceneTransitionManager.Instance.SetTransitionData(
            SceneManager.GetActiveScene().name,
            targetSpawnId
        );

        SceneManager.LoadScene(nextSceneName);
    }
}