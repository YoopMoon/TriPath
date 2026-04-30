using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelInitializer : MonoBehaviour
{
    private IEnumerator Start()
    {
        string currentLevelID = SceneManager.GetActiveScene().name;

        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.MarkVisited(currentLevelID);

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.ResetMetrics();

        // Esperamos un frame para que otros Start() del nivel se ejecuten
        // y los coleccionables persistidos que deban destruirse desaparezcan.
        yield return null;

        ItemCollected[] items = FindObjectsByType<ItemCollected>();

        int totalCoins = 0;

        foreach (ItemCollected item in items)
        {
            if (item.collectibleType == ItemCollected.CollectibleType.Coin)
                totalCoins++;
        }

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.SetTotalCoins(totalCoins);

        Debug.Log($"[LevelInitializer] Nivel: {currentLevelID} | TotalCoins: {totalCoins}");
    }
}