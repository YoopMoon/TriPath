using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class RankingStorage
{
    private const string FileName = "ranking_players.json";

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static RankingData LoadRanking()
    {
        if (!File.Exists(FilePath))
            return new RankingData();

        string json = File.ReadAllText(FilePath);

        if (string.IsNullOrEmpty(json))
            return new RankingData();

        RankingData data = JsonUtility.FromJson<RankingData>(json);

        if (data == null)
            data = new RankingData();

        if (data.entries == null)
            data.entries = new List<RankingEntry>();

        return data;
    }

    public static void SaveOrReplaceEntry(RankingEntry newEntry)
    {
        if (newEntry == null)
            return;

        if (string.IsNullOrWhiteSpace(newEntry.playerName))
            newEntry.playerName = "Player";

        RankingData data = LoadRanking();

        string normalizedName = NormalizeName(newEntry.playerName);

        data.entries.RemoveAll(entry => NormalizeName(entry.playerName) == normalizedName);

        data.entries.Add(newEntry);

        data.entries = SortEntries(data.entries);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);

        Debug.Log($"[RankingStorage] Ranking saved in: {FilePath}");
    }

    public static List<RankingEntry> GetSortedEntries()
    {
        RankingData data = LoadRanking();
        return SortEntries(data.entries);
    }

    public static void ClearRanking()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }

    private static List<RankingEntry> SortEntries(List<RankingEntry> entries)
    {
        if (entries == null)
            return new List<RankingEntry>();

        return entries
            .OrderByDescending(entry => entry.finalScore)
            .ThenByDescending(entry => entry.totalCoinsCollected)
            .ThenBy(entry => entry.totalDamageTaken)
            .ThenBy(entry => entry.totalTime)
            .ToList();
    }

    private static string NormalizeName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            return "player";

        return playerName.Trim().ToLowerInvariant();
    }
}