using System.Collections.Generic;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    public static LevelProgressManager Instance { get; private set; }

    public Dictionary<string, LevelProgressData> levels = new Dictionary<string, LevelProgressData>();

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

    public void MarkVisited(string levelID)
    {
        LevelProgressData data = GetOrCreate(levelID);
        data.visited = true;
    }

    public void MarkCompleted(string levelID)
    {
        LevelProgressData data = GetOrCreate(levelID);
        data.completed = true;
    }

    public bool HasBeenVisited(string levelID)
    {
        return GetOrCreate(levelID).visited;
    }

    public bool HasBeenCompleted(string levelID)
    {
        return GetOrCreate(levelID).completed;
    }

    private LevelProgressData GetOrCreate(string levelID)
    {
        if (!levels.ContainsKey(levelID))
        {
            levels[levelID] = new LevelProgressData
            {
                levelID = levelID,
                visited = false,
                completed = false
            };
        }

        return levels[levelID];
    }

    public void ResetProgress()
    {
        levels.Clear();
    }
}