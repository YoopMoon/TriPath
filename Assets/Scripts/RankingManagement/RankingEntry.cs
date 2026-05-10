using System;

[Serializable]
public class RankingEntry
{
    public string playerName;
    public string characterName;

    public int totalCoinsCollected;
    public int totalCoinsAvailable;
    public int totalDamageTaken;

    public float totalTime;
    public int finalScore;
}