using UnityEngine;

public class HeadEnemy : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float knockbackX = 2.5f;
    [SerializeField] private float knockbackY = 2.5f;

    [Header("Damage Collider")]
    [SerializeField] private Collider2D damageCollider;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayerFromCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayerFromCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayerFromTrigger(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayerFromTrigger(other);
    }

    private void TryDamagePlayerFromCollision(Collision2D collision)
    {
        if (!HasCollisionWithDamageCollider(collision))
            return;

        TryDamagePlayer(collision.transform);
    }

    private void TryDamagePlayerFromTrigger(Collider2D other)
    {
        if (damageCollider == null)
            return;

        if (!damageCollider.IsTouching(other))
            return;

        TryDamagePlayer(other.transform);
    }

    private bool HasCollisionWithDamageCollider(Collision2D collision)
    {
        if (damageCollider == null)
            return false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.collider == damageCollider || contact.otherCollider == damageCollider)
                return true;
        }

        return false;
    }

    private void TryDamagePlayer(Transform target)
    {
        if (!target.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        float directionX = target.position.x > transform.position.x ? 1f : -1f;
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