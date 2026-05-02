using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelInitializer : MonoBehaviour
{
    private string lastInitializedSceneName = string.Empty;
    private Coroutine initializeCoroutine;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (IsLevelScene(activeScene.name))
            InitializeScene(activeScene.name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si entramos en una escena que no es nivel, por ejemplo MainMenu,
        // limpiamos el último nivel inicializado. Así una nueva run puede
        // volver a inicializar Map1_Level1 correctamente.
        if (!IsLevelScene(scene.name))
        {
            lastInitializedSceneName = string.Empty;
            return;
        }

        InitializeScene(scene.name);
    }

    private void InitializeScene(string sceneName)
    {
        if (lastInitializedSceneName == sceneName)
        {
            Debug.Log($"[LevelInitializer] {sceneName} already initialized. Skipping duplicate initialization.");
            return;
        }

        lastInitializedSceneName = sceneName;

        if (initializeCoroutine != null)
            StopCoroutine(initializeCoroutine);

        initializeCoroutine = StartCoroutine(InitializeLevel(sceneName));
    }

    private IEnumerator InitializeLevel(string currentLevelID)
    {
        Debug.Log($"[LevelInitializer] Initializing level: {currentLevelID}");

        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.MarkVisited(currentLevelID);

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.ResetMetrics();

        // Esperamos un frame para que los objetos de la escena estén disponibles.
        yield return null;

        ItemCollected[] items = FindObjectsByType<ItemCollected>(FindObjectsInactive.Include);

        int totalCoins = 0;

        foreach (ItemCollected item in items)
        {
            if (item == null)
                continue;

            if (item.collectibleType == ItemCollected.CollectibleType.Coin)
                totalCoins++;
        }

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.SetTotalCoins(totalCoins);

        Debug.Log($"[LevelInitializer] Nivel: {currentLevelID} | TotalCoins: {totalCoins}");

        initializeCoroutine = null;
    }

    private bool IsLevelScene(string sceneName)
    {
        return !string.IsNullOrEmpty(sceneName) && sceneName.StartsWith("Map");
    }
}