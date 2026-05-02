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

    [Header("Time Criteria")]
    [SerializeField] private float goodTimeThreshold = 120f;
    [SerializeField] private float acceptableTimeThreshold = 180f;

    [Header("Damage Criteria")]
    [SerializeField] private int lowDamageThreshold = 2;
    [SerializeField] private int mediumDamageThreshold = 4;

    [Header("Coin Criteria")]
    [SerializeField] private float highCoinPercentageThreshold = 0.8f;
    [SerializeField] private float mediumCoinPercentageThreshold = 0.5f;

    [Header("Last Evaluation Debug")]
    [SerializeField] private int lastScore;
    [SerializeField] private float lastTime;
    [SerializeField] private int lastDamage;
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
        float coinPercentage = LevelMetrics.Instance.CoinPercentage;

        lastTime = time;
        lastDamage = damage;
        lastCoinPercentage = coinPercentage;

        score += EvaluateTimeScore(time);
        score += EvaluateDamageScore(damage);
        score += EvaluateCoinScore(coinPercentage);

        lastScore = score;

        currentDifficulty = GetDifficultyFromScore(score, damage);

        Debug.Log(
            $"Adaptive Difficulty -> " +
            $"Time: {time:0.00}s, " +
            $"Damage: {damage}, " +
            $"Coins: {coinPercentage * 100f:0.##}%, " +
            $"Score: {score}/6, " +
            $"New Difficulty: {currentDifficulty}"
        );
    }

    private int EvaluateTimeScore(float time)
    {
        if (time <= goodTimeThreshold)
            return 2;

        if (time <= acceptableTimeThreshold)
            return 1;

        return 0;
    }

    private int EvaluateDamageScore(int damage)
    {
        if (damage <= lowDamageThreshold)
            return 2;

        if (damage <= mediumDamageThreshold)
            return 1;

        return 0;
    }

    private int EvaluateCoinScore(float coinPercentage)
    {
        if (coinPercentage >= highCoinPercentageThreshold)
            return 2;

        if (coinPercentage >= mediumCoinPercentageThreshold)
            return 1;

        return 0;
    }

    private AdaptiveDifficulty GetDifficultyFromScore(int score, int damage)
    {
        // Regla de seguridad:
        // si el jugador recibe mucho daño, no subimos a Hard aunque haya ido rápido
        // o haya recogido muchas monedas.
        if (damage > mediumDamageThreshold)
            return AdaptiveDifficulty.Normal;

        // Con 3 criterios, la puntuación máxima es 6.
        if (score >= 5)
            return AdaptiveDifficulty.Hard;

        if (score >= 3)
            return AdaptiveDifficulty.Normal;

        return AdaptiveDifficulty.Easy;
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
        lastCoinPercentage = 0f;
    }
}