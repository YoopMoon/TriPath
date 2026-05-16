using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentBackground : MonoBehaviour
{
    public static PersistentBackground Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool destroyDuplicatedBackgrounds = true;
    [SerializeField] private string[] validScenePrefixes;

    private void Awake()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;

        if (!IsValidForScene(activeSceneName))
        {
            Destroy(gameObject);
            return;
        }

        if (Instance != null && Instance != this)
        {
            if (!Instance.IsValidForScene(activeSceneName))
            {
                Instance.DestroyBackground();
                Instance = this;
                DontDestroyOnLoad(gameObject);
                return;
            }

            if (destroyDuplicatedBackgrounds)
                Destroy(gameObject);

            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsValidForScene(scene.name))
            DestroyBackground();
    }

    public void DestroyBackground()
    {
        if (Instance == this)
            Instance = null;

        Destroy(gameObject);
    }

    public bool IsValidForScene(string sceneName)
    {
        if (validScenePrefixes == null || validScenePrefixes.Length == 0)
            return true;

        foreach (string validScenePrefix in validScenePrefixes)
        {
            if (string.IsNullOrWhiteSpace(validScenePrefix))
                continue;

            if (sceneName.StartsWith(validScenePrefix))
                return true;
        }

        return false;
    }
}
