using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMetrics : MonoBehaviour
{
    public static LevelMetrics Instance { get; private set; }

    private float levelStartTime;
    private int damageTaken;
    private int deaths;

    // Métricas del nivel actual.
    // Estas son las que usa el sistema de dificultad adaptativa.
    private int coinsCollected;
    private int totalCoins;

    // Evita guardar dos veces el resumen del mismo nivel en la misma visita.
    private bool currentLevelSummarySaved;
    private string currentLevelSceneName = string.Empty;

    // Resumen acumulado por nivel para poder mostrar el resumen final del mapa.
    // Key: nombre de escena, por ejemplo "Map1_Level1".
    private readonly Dictionary<string, LevelCoinSummary> levelCoinSummaries = new();

    public float ElapsedTime => Time.time - levelStartTime;
    public int DamageTaken => damageTaken;
    public int Deaths => deaths;
    public int CoinsCollected => coinsCollected;
    public int TotalCoins => totalCoins;

    public float CoinPercentage
    {
        get
        {
            if (totalCoins <= 0)
                return 0f;

            return (float)coinsCollected / totalCoins;
        }
    }

    public class LevelCoinSummary
    {
        public string sceneName;
        public string mapName;
        public int coinsCollected;
        public int totalCoins;

        public LevelCoinSummary(string sceneName, string mapName, int coinsCollected, int totalCoins)
        {
            this.sceneName = sceneName;
            this.mapName = mapName;
            this.coinsCollected = coinsCollected;
            this.totalCoins = totalCoins;
        }
    }

    public class MapCoinSummary
    {
        public string mapName;
        public int coinsCollected;
        public int totalCoins;

        public float CoinPercentage
        {
            get
            {
                if (totalCoins <= 0)
                    return 0f;

                return (float)coinsCollected / totalCoins;
            }
        }

        public MapCoinSummary(string mapName)
        {
            this.mapName = mapName;
        }
    }

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

    public void ResetMetrics()
    {
        currentLevelSceneName = SceneManager.GetActiveScene().name;

        levelStartTime = Time.time;
        damageTaken = 0;
        deaths = 0;

        coinsCollected = 0;
        totalCoins = 0;

        currentLevelSummarySaved = false;

        Debug.Log($"[LevelMetrics] Reset metrics for {currentLevelSceneName}");
    }

    public void RegisterDamageTaken(int amount)
    {
        damageTaken += amount;
    }

    public void RegisterDeath()
    {
        deaths++;
    }

    public void RegisterCoinCollected()
    {
        coinsCollected++;

        if (totalCoins > 0)
            coinsCollected = Mathf.Min(coinsCollected, totalCoins);

        //Debug.Log($"[LevelMetrics] Coin collected: {coinsCollected}/{totalCoins}");
    }

    public void SetTotalCoins(int amount)
    {
        totalCoins = Mathf.Max(0, amount);

        if (totalCoins > 0)
            coinsCollected = Mathf.Min(coinsCollected, totalCoins);

        // Debug.Log($"[LevelMetrics] Total coins set for {currentLevelSceneName}: {totalCoins}");
    }

    public void SaveCurrentLevelCoinSummary()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != currentLevelSceneName)
        {
            currentLevelSceneName = sceneName;
            currentLevelSummarySaved = false;
        }

        if (currentLevelSummarySaved)
        {
            Debug.Log($"[LevelMetrics] Summary for {sceneName} was already saved. Skipping duplicate save.");
            return;
        }

        string mapName = GetMapNameFromScene(sceneName);

        int safeTotalCoins = Mathf.Max(0, totalCoins);
        int safeCoinsCollected = Mathf.Clamp(coinsCollected, 0, safeTotalCoins);

        if (levelCoinSummaries.TryGetValue(sceneName, out LevelCoinSummary existingSummary))
        {
            existingSummary.totalCoins = Mathf.Max(existingSummary.totalCoins, safeTotalCoins);

            // Si el jugador vuelve a un nivel anterior, solo podrá recoger monedas
            // que aún no estaban recogidas gracias al sistema de persistencia.
            // Por eso aquí sumamos lo recogido en esta visita, limitando el resultado
            // para que nunca supere el total del nivel.
            existingSummary.coinsCollected = Mathf.Clamp(
                existingSummary.coinsCollected + safeCoinsCollected,
                0,
                existingSummary.totalCoins
            );
        }
        else
        {
            levelCoinSummaries.Add(
                sceneName,
                new LevelCoinSummary(sceneName, mapName, safeCoinsCollected, safeTotalCoins)
            );
        }

        currentLevelSummarySaved = true;

        Debug.Log($"[LevelMetrics] Saved {sceneName}: {safeCoinsCollected}/{safeTotalCoins}");
    }

    public void LogLevelCoinSummaries()
    {
        Debug.Log("===== LEVEL COIN SUMMARY =====");

        foreach (LevelCoinSummary summary in levelCoinSummaries.Values)
            Debug.Log($"{summary.sceneName}: {summary.coinsCollected}/{summary.totalCoins}");

        Debug.Log("==============================");
    }

    public void LogCurrentMapCoinSummary()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        string currentMapName = GetMapNameFromScene(currentSceneName);

        MapCoinSummary summary = GetMapCoinSummary(currentMapName);
        float percentage = summary.CoinPercentage * 100f;
        
        /*
        Debug.Log("===== MAP COIN SUMMARY =====");
        Debug.Log($"{summary.mapName}: {summary.coinsCollected}/{summary.totalCoins} coins collected ({percentage:0.##}%)");
        Debug.Log("============================");
        */
    }

    private MapCoinSummary GetMapCoinSummary(string mapName)
    {
        MapCoinSummary mapSummary = new MapCoinSummary(mapName);

        foreach (LevelCoinSummary levelSummary in levelCoinSummaries.Values)
        {
            if (levelSummary.mapName != mapName)
                continue;

            mapSummary.coinsCollected += levelSummary.coinsCollected;
            mapSummary.totalCoins += levelSummary.totalCoins;
        }

        mapSummary.coinsCollected = Mathf.Clamp(
            mapSummary.coinsCollected,
            0,
            mapSummary.totalCoins
        );

        return mapSummary;
    }

    private string GetMapNameFromScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return string.Empty;

        // Map1_Level2 -> Map1
        int separatorIndex = sceneName.IndexOf('_');

        if (separatorIndex < 0)
            return sceneName;

        return sceneName.Substring(0, separatorIndex);
    }

    public void ResetProgress()
    {
        levelCoinSummaries.Clear();

        levelStartTime = Time.time;
        damageTaken = 0;
        deaths = 0;
        coinsCollected = 0;
        totalCoins = 0;

        currentLevelSummarySaved = false;
        currentLevelSceneName = string.Empty;

        Debug.Log("[LevelMetrics] Progress reset.");
    }
}