using UnityEngine;

[System.Serializable]
public class DifficultySettings
{
    public AdaptiveDifficulty difficulty;

    [Header("Player pressure")]
    public float enemyDamageMultiplier = 1f;
    public float environmentDamageMultiplier = 1f;
    public int fruitHealAmount = 1;

    [Header("Enemy movement")]
    public float enemySpeedMultiplier = 1f;
    public float enemyWaitDurationMultiplier = 1f;

    [Header("Enemy health")]
    public float enemyHealthMultiplier = 1f;
}