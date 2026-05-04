using UnityEngine;

public class DamagePlayerOnCollision : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private Vector2 knockbackForce = new Vector2(2.5f, 2.5f);

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
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (!collision.gameObject.TryGetComponent(out PlayerHealth playerHealth))
            return;

        Vector2 appliedKnockback = GetKnockbackForce(collision.transform);
        int finalDamage = GetAdaptedDamage();

        playerHealth.TakeDamage(finalDamage, appliedKnockback);
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

    private Vector2 GetKnockbackForce(Transform playerTransform)
    {
        float horizontalDirection = playerTransform.position.x < transform.position.x ? -1f : 1f;

        return new Vector2(
            knockbackForce.x * horizontalDirection,
            knockbackForce.y
        );
    }
}