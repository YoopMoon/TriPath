using UnityEngine;

public class PlayerSceneSpawner : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;

    private void Start()
    {
        if (SceneTransitionManager.instance == null)
            return;

        string targetSpawnId = SceneTransitionManager.instance.targetSpawnId;

        if (string.IsNullOrEmpty(targetSpawnId))
            return;

        //SceneSpawnPoint[] spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsSortMode.None);
        SceneSpawnPoint[] spawnPoints = FindObjectsByType<SceneSpawnPoint>();

        foreach (SceneSpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.spawnId == targetSpawnId)
            {   
                if (targetSpawnId == "SpawnFromRight")
                {
                    spriteRenderer.flipX = true;
                }
                transform.position = spawnPoint.transform.position;
                SceneTransitionManager.instance.ClearTransitionData();
                return;
            }
        }

        Debug.LogWarning("No se encontró un SceneSpawnPoint con spawnId = " + targetSpawnId);
    }
}