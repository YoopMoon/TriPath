using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemCollected : MonoBehaviour
{
    public enum CollectibleType
    {
        Fruit,
        Coin
    }

    [Header("Collectible Settings")]
    public CollectibleType collectibleType = CollectibleType.Fruit;

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

        if (CollectibleStateManager.Instance != null)
        {
            CollectibleStateManager.Instance.MarkAsCollected(itemID);
        }

        switch (collectibleType)
        {
            case CollectibleType.Coin:
                CoinsManager coinsManager = FindAnyObjectByType<CoinsManager>();
                if (coinsManager != null)
                {
                    coinsManager.CoinCollected();
                }
                break;

            case CollectibleType.Fruit:
                // futura lógica de curación
                break;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (collectedEffect != null)
            collectedEffect.SetActive(true);

        Destroy(gameObject, 0.5f);
    }
}