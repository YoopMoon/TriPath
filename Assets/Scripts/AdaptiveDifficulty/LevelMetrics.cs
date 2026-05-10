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

    // Evita guardar dos veces el resumen de monedas del mismo nivel en la misma visita.
    private bool currentLevelSummarySaved;
    private string currentLevelSceneName = string.Empty;

    // Resumen acumulado por nivel para poder mostrar el resumen final del mapa/ranking.
    // Key: nombre de escena, por ejemplo "Map1_Level1".
    private readonly Dictionary<string, LevelCoinSummary> levelCoinSummaries = new();
    private readonly Dictionary<string, LevelPerformanceSummary> levelPerformanceSummaries = new();

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

    public class LevelPerformanceSummary
    {
        public string sceneName;
        public string mapName;
        public int damageTaken;
        public float elapsedTime;

        public LevelPerformanceSummary(string sceneName, string mapName, int damageTaken, float elapsedTime)
        {
            this.sceneName = sceneName;
            this.mapName = mapName;
            this.damageTaken = damageTaken;
            this.elapsedTime = elapsedTime;
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
    }

    public void SetTotalCoins(int amount)
    {
        totalCoins = Mathf.Max(0, amount);

        if (totalCoins > 0)
            coinsCollected = Mathf.Min(coinsCollected, totalCoins);
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
            Debug.Log($"[LevelMetrics] Coin summary for {sceneName} was already saved. Skipping duplicate save.");
            return;
        }

        string mapName = GetMapNameFromScene(sceneName);

        int safeTotalCoins = Mathf.Max(0, totalCoins);
        int safeCoinsCollected = Mathf.Clamp(coinsCollected, 0, safeTotalCoins);

        if (levelCoinSummaries.TryGetValue(sceneName, out LevelCoinSummary existingSummary))
        {
            existingSummary.totalCoins = Mathf.Max(existingSummary.totalCoins, safeTotalCoins);

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

        Debug.Log($"[LevelMetrics] Saved coins {sceneName}: {safeCoinsCollected}/{safeTotalCoins}");
    }

    public void SaveCurrentLevelPerformanceSummary()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string mapName = GetMapNameFromScene(sceneName);

        int safeDamageTaken = Mathf.Max(0, damageTaken);
        float safeElapsedTime = Mathf.Max(0f, ElapsedTime);

        if (levelPerformanceSummaries.TryGetValue(sceneName, out LevelPerformanceSummary existingSummary))
        {
            existingSummary.damageTaken = safeDamageTaken;
            existingSummary.elapsedTime = safeElapsedTime;
        }
        else
        {
            levelPerformanceSummaries.Add(
                sceneName,
                new LevelPerformanceSummary(sceneName, mapName, safeDamageTaken, safeElapsedTime)
            );
        }

        Debug.Log($"[LevelMetrics] Saved performance {sceneName}: Damage {safeDamageTaken}, Time {safeElapsedTime:0.00}s");
    }

    public int GetTotalCoinsCollected()
    {
        int total = 0;

        foreach (LevelCoinSummary summary in levelCoinSummaries.Values)
            total += summary.coinsCollected;

        return total;
    }

    public int GetTotalCoinsAvailable()
    {
        int total = 0;

        foreach (LevelCoinSummary summary in levelCoinSummaries.Values)
            total += summary.totalCoins;

        return total;
    }

    public int GetTotalDamageTaken()
    {
        int total = 0;

        foreach (LevelPerformanceSummary summary in levelPerformanceSummaries.Values)
            total += summary.damageTaken;

        return total;
    }

    public float GetTotalElapsedTime()
    {
        float total = 0f;

        foreach (LevelPerformanceSummary summary in levelPerformanceSummaries.Values)
            total += summary.elapsedTime;

        return total;
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

        Debug.Log("===== MAP COIN SUMMARY =====");
        Debug.Log($"{summary.mapName}: {summary.coinsCollected}/{summary.totalCoins} coins collected ({percentage:0.##}%)");
        Debug.Log("============================");
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
        levelPerformanceSummaries.Clear();

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