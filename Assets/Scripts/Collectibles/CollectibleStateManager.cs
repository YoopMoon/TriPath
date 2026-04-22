using System.Collections.Generic;
using UnityEngine;

public class CollectibleStateManager : MonoBehaviour
{
    public static CollectibleStateManager Instance;

    private HashSet<string> collectedItems = new HashSet<string>();

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

    public void MarkAsCollected(string itemId)
    {
        collectedItems.Add(itemId);
    }

    public bool IsCollected(string itemId)
    {
        return collectedItems.Contains(itemId);
    }

    public void ResetProgress()
    {
        collectedItems.Clear();
    }
}