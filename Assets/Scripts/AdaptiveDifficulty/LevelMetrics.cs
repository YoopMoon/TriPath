using UnityEngine;

public class LevelMetrics : MonoBehaviour
{
    public static LevelMetrics Instance { get; private set; }

    private float levelStartTime;
    private int damageTaken;
    private int deaths;
    private int coinsCollected;
    private int totalCoins;

    public float ElapsedTime => Time.time - levelStartTime;
    public int DamageTaken => damageTaken;
    public int Deaths => deaths;
    public int CoinsCollected => coinsCollected;
    public int TotalCoins => totalCoins;

    public float CoinPercentage
    {
        get
        {
            if (totalCoins <= 0) return 0f;
            return (float)coinsCollected / totalCoins;
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
    }

    private void Start()
    {
        ResetMetrics();
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
    }

    public void SetTotalCoins(int amount)
    {
        totalCoins = amount;
    }

    public void ResetMetrics()
    {
        levelStartTime = Time.time;
        damageTaken = 0;
        deaths = 0;
        coinsCollected = 0;
        totalCoins = 0;
    }
}