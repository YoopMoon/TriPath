using UnityEngine;

public class AdaptiveDifficultyManager : MonoBehaviour
{
    public static AdaptiveDifficultyManager Instance { get; private set; }

    [Header("Current Difficulty")]
    [SerializeField] private AdaptiveDifficulty currentDifficulty = AdaptiveDifficulty.Normal;
    [SerializeField] private AdaptiveDifficulty defaultDifficulty = AdaptiveDifficulty.Normal;

    [Header("Difficulty Settings")]
    [SerializeField] private DifficultySettings easySettings;
    [SerializeField] private DifficultySettings normalSettings;
    [SerializeField] private DifficultySettings hardSettings;

    [Header("Last Evaluation Debug")]
    [SerializeField] private int lastScore;
    [SerializeField] private float lastTime;
    [SerializeField] private int lastDamage;
    [SerializeField] private int lastDeaths;
    [SerializeField] private float lastCoinPercentage;

    public AdaptiveDifficulty CurrentDifficulty => currentDifficulty;
    public int LastScore => lastScore;

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

    public void EvaluatePlayerPerformance()
    {
        if (LevelMetrics.Instance == null)
            return;

        int score = 0;

        float time = LevelMetrics.Instance.ElapsedTime;
        int damage = LevelMetrics.Instance.DamageTaken;
        int deaths = LevelMetrics.Instance.Deaths;
        float coinPercentage = LevelMetrics.Instance.CoinPercentage;

        lastTime = time;
        lastDamage = damage;
        lastDeaths = deaths;
        lastCoinPercentage = coinPercentage;

        if (time <= 120f) score += 2;
        else if (time <= 180f) score += 1;

        if (damage <= 4) score += 2;
        else if (damage <= 8) score += 1;

        if (coinPercentage >= 0.8f) score += 2;
        else if (coinPercentage >= 0.5f) score += 1;

        if (deaths == 0) score += 2;
        else if (deaths == 1) score += 1;

        lastScore = score;

        if (score >= 7)
            currentDifficulty = AdaptiveDifficulty.Hard;
        else if (score >= 4)
            currentDifficulty = AdaptiveDifficulty.Normal;
        else
            currentDifficulty = AdaptiveDifficulty.Easy;

        Debug.Log($"Adaptive Difficulty -> Score: {score}, New Difficulty: {currentDifficulty}");
    }

    public DifficultySettings GetCurrentSettings()
    {
        return currentDifficulty switch
        {
            AdaptiveDifficulty.Easy => easySettings,
            AdaptiveDifficulty.Normal => normalSettings,
            AdaptiveDifficulty.Hard => hardSettings,
            _ => normalSettings
        };
    }

    public void SetDifficulty(AdaptiveDifficulty newDifficulty)
    {
        currentDifficulty = newDifficulty;
    }

    public void ResetProgress()
    {
        currentDifficulty = defaultDifficulty;

        lastScore = 0;
        lastTime = 0f;
        lastDamage = 0;
        lastDeaths = 0;
        lastCoinPercentage = 0f;
    }
}