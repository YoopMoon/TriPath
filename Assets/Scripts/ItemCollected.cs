using UnityEngine;

public class ItemCollected : MonoBehaviour
{
    // enum para distinguir el tipo de coleccionable desde el inspector
    // así el mismo script sirve para monedas, frutas o cualquier tipo que se añada después
    public enum CollectibleType
    {
        Fruit,
        Coin
    }

    [Header("Collectible Settings")]
    public CollectibleType collectibleType = CollectibleType.Fruit;

    // evita que el trigger se procese más de una vez antes de que el objeto se destruya
    private bool collected = false;
    private SpriteRenderer spriteRenderer;
    private GameObject collectedEffect;

    private void Awake()
    {
        // cacheo las referencias en Awake para no buscarlas cada vez que se recoge el objeto
        spriteRenderer = GetComponent<SpriteRenderer>();

        // compruebo que el objeto tiene hijo antes de intentar acceder a él
        // así no peta si por error se usa el script en un objeto sin efecto de recogida
        if (transform.childCount > 0)
        {
            collectedEffect = transform.GetChild(0).gameObject;
            collectedEffect.SetActive(false); // me aseguro de que el efecto empiece desactivado
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // si ya se recogió, salgo inmediatamente para evitar dobles conteos
        if (collected)
            return;

        // solo proceso la colisión si es el jugador
        if (!collision.CompareTag("Player"))
            return;

        collected = true;

        // ejecuto la lógica específica según el tipo de coleccionable
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
                // lógica de vida o curación, pendiente de implementar
                //PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
                //if (playerHealth != null)
                //{
                //    playerHealth.Heal(1);
                //}
                break;
        }

        // oculto el sprite y activo el efecto de recogida antes de destruir el objeto
        // el null check es por si el script se usa en un objeto sin SpriteRenderer o sin efecto
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (collectedEffect != null)
            collectedEffect.SetActive(true);

        // destruyo con un pequeño delay para que el efecto de recogida tenga tiempo de verse
        Destroy(gameObject, 0.5f);
    }
}