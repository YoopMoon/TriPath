using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundResetter : MonoBehaviour
{
    private void Awake()
    {
        if (PersistentBackground.Instance != null &&
            !PersistentBackground.Instance.IsValidForScene(SceneManager.GetActiveScene().name))
        {
            PersistentBackground.Instance.DestroyBackground();
        }
    }
}
