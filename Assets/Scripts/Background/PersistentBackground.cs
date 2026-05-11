using UnityEngine;

public class PersistentBackground : MonoBehaviour
{
    public static PersistentBackground Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool destroyDuplicatedBackgrounds = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (destroyDuplicatedBackgrounds)
                Destroy(gameObject);

            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void DestroyBackground()
    {
        if (Instance == this)
            Instance = null;

        Destroy(gameObject);
    }
}