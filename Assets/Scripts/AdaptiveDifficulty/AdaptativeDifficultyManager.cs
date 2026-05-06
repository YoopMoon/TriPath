using UnityEngine;
using System;

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
    [SerializeField] private float goodTimeThreshold = 25f;
    [SerializeField] private float acceptableTimeThreshold = 40f;

    [Header("Damage Criteria")]
    [SerializeField] private int lowDamageThreshold = 1;
    [SerializeField] private int mediumDamageThreshold = 3;

    [Header("Coin Criteria")]
    [SerializeField] private float highCoinPercentageThreshold = 0.75f;
    [SerializeField] private float mediumCoinPercentageThreshold = 0.4f;

    [Header("Last Evaluation Debug")]
    [SerializeField] private int lastScore;
    [SerializeField] private float lastTime;
    [SerializeField] private int lastDamage;
    [SerializeField] private float lastCoinPercentage;

    public event Action<AdaptiveDifficulty> OnDifficultyChanged;

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

        SetDifficulty(GetDifficultyFromScore(score, damage));

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
        AdaptiveDifficulty targetDifficulty;

        if (damage > mediumDamageThreshold)
        {
            targetDifficulty = AdaptiveDifficulty.Easy;
        }
        else if (damage > lowDamageThreshold && score < 4)
        {
            targetDifficulty = AdaptiveDifficulty.Easy;
        }
        else if (score >= 5)
        {
            targetDifficulty = AdaptiveDifficulty.Hard;
        }
        else if (score >= 3)
        {
            targetDifficulty = AdaptiveDifficulty.Normal;
        }
        else
        {
            targetDifficulty = AdaptiveDifficulty.Easy;
        }

        return LimitDifficultyJump(currentDifficulty, targetDifficulty);
    }

    private AdaptiveDifficulty LimitDifficultyJump(
        AdaptiveDifficulty current, 
        AdaptiveDifficulty target)
    {
        if (current == AdaptiveDifficulty.Easy && target == AdaptiveDifficulty.Hard)
            return AdaptiveDifficulty.Normal;

        if (current == AdaptiveDifficulty.Hard && target == AdaptiveDifficulty.Easy)
            return AdaptiveDifficulty.Normal;

        return target;
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
        OnDifficultyChanged?.Invoke(currentDifficulty);
    }

    public void ResetProgress()
    {
        SetDifficulty(defaultDifficulty);

        lastScore = 0;
        lastTime = 0f;
        lastDamage = 0;
        lastCoinPercentage = 0f;
    }
}