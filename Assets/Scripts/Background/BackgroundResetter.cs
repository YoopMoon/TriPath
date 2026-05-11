using UnityEngine;

public class BackgroundResetter : MonoBehaviour
{
    private void Awake()
    {
        if (PersistentBackground.Instance != null)
            PersistentBackground.Instance.DestroyBackground();
    }
}