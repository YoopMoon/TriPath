using UnityEngine;

public class HeadEnemy : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float knockbackX = 2.5f;
    [SerializeField] private float knockbackY = 2.5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void TryDamagePlayer(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = collision.transform.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        float directionX = collision.transform.position.x > transform.position.x ? 1f : -1f;
        Vector2 knockbackForce = new Vector2(directionX * knockbackX, knockbackY);

        int finalDamage = GetAdaptedDamage();
        playerHealth.TakeDamage(finalDamage, knockbackForce);
    }

    private int GetAdaptedDamage()
    {
        float damageMultiplier = 1f;

        if (AdaptiveDifficultyManager.Instance != null)
        {
            DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();
            damageMultiplier = settings.enemyDamageMultiplier;
        }

        int adaptedDamage = Mathf.RoundToInt(damageAmount * damageMultiplier);
        return Mathf.Max(1, adaptedDamage);
    }
}