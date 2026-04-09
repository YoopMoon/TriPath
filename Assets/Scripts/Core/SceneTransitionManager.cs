using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    public string previousScene { get; private set; }
    public string targetSpawnId { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
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