using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

public class ItemCollected : MonoBehaviour
{
    public enum CollectibleType
    {
        Fruit,
        Coin
    }

    [Header("Collectible Settings")]
    public CollectibleType collectibleType = CollectibleType.Fruit;

    [Header("Audio")]
    [SerializeField] private AudioClip collectedClip;
    [SerializeField] private float collectedVolume = 1f;

    private bool collected = false;
    private SpriteRenderer spriteRenderer;
    private GameObject collectedEffect;
    private string itemID;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (transform.childCount > 0)
        {
            collectedEffect = transform.GetChild(0).gameObject;
            collectedEffect.SetActive(false);
        }

        itemID = SceneManager.GetActiveScene().name + "_" + gameObject.name;
    }

    private void Start()
    {
        if (CollectibleStateManager.Instance != null &&
            CollectibleStateManager.Instance.IsCollected(itemID))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected)
            return;

        if (!collision.CompareTag("Player"))
            return;

        collected = true;

        PlayCollectedSound();

        if (CollectibleStateManager.Instance != null)
            CollectibleStateManager.Instance.MarkAsCollected(itemID);

        PlayerHealth playerHealth = collision.transform.GetComponent<PlayerHealth>();

        switch (collectibleType)
        {
            case CollectibleType.Coin:
                if (SceneTransitionManager.instance != null)
                    SceneTransitionManager.instance.AddPlayerCoins(1);

                if (LevelMetrics.Instance != null)
                {
                    LevelMetrics.Instance.RegisterCoinCollected();
                    Debug.Log($"[ItemCollected] Coin recogida. CoinsCollected: {LevelMetrics.Instance.CoinsCollected} / {LevelMetrics.Instance.TotalCoins}");
                }
                break;

            case CollectibleType.Fruit:
                if (playerHealth != null)
                {
                    int healAmount = 1;

                    if (AdaptiveDifficultyManager.Instance != null)
                    {
                        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();
                        healAmount = settings.fruitHealAmount;
                    }

                    playerHealth.Heal(healAmount);
                }
                break;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (collectedEffect != null)
            collectedEffect.SetActive(true);

        Destroy(gameObject, 0.5f);
    }

    private void PlayCollectedSound()
    {
        if (collectedClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(collectedClip, collectedVolume);
    }
}