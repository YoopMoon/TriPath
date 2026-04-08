using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    public string previousScene { get; private set; }
    public string targetSpawnId { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetTransitionData(string fromScene, string spawnId)
    {
        previousScene = fromScene;
        targetSpawnId = spawnId;
    }

    public void ClearTransitionData()
    {
        previousScene = "";
        targetSpawnId = "";
    }
}