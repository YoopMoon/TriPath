using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    public enum BulletDirection
    {
        Left,
        Right,
        Down,
        Up
    }

    [Header("Bullet Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float lifeTime = 2f;

    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float knockbackX = 2.5f;
    [SerializeField] private float knockbackY = 2.5f;

    [Header("Direction")]
    [SerializeField] private BulletDirection bulletDirection = BulletDirection.Left;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Adaptive Difficulty")]
    [SerializeField] private bool useAdaptiveDifficulty = true;
    [SerializeField] private bool adaptLifeTime = false;

    private float adaptedSpeed;
    private float adaptedLifeTime;
    private bool isDestroyed;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        ApplyDifficultySettings();
    }

    private void Start()
    {
        MoveBullet();
        Destroy(gameObject, adaptedLifeTime);
    }

    private void MoveBullet()
    {
        Vector2 direction = GetDirection();

        if (rb != null)
        {
            rb.linearVelocity = direction * adaptedSpeed;
            return;
        }

        Debug.LogWarning("[BulletPlant] La bala no tiene Rigidbody2D.");
    }

    private Vector2 GetDirection()
    {
        switch (bulletDirection)
        {
            case BulletDirection.Left:
                return Vector2.left;

            case BulletDirection.Right:
                return Vector2.right;

            case BulletDirection.Down:
                return Vector2.down;

            case BulletDirection.Up:
                return Vector2.up;

            default:
                return Vector2.left;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryHandleContact(collision.gameObject, collision.transform);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleContact(collision.gameObject, collision.transform);
    }

    private void TryHandleContact(GameObject otherObject, Transform otherTransform)
    {
        if (isDestroyed)
            return;

        if (otherObject == null)
            return;

        if (otherObject.CompareTag("Player"))
        {
            DamagePlayer(otherTransform);
            DestroyBullet();
            return;
        }

        if (otherObject.CompareTag("Floor"))
        {
            DestroyBullet();
        }
    }

    private void DamagePlayer(Transform playerTransform)
    {
        if (playerTransform == null)
            return;

        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        Vector2 knockbackForce = GetKnockbackForce(playerTransform);
        int finalDamage = GetAdaptedDamage();

        playerHealth.TakeDamage(finalDamage, knockbackForce);
    }

    private Vector2 GetKnockbackForce(Transform playerTransform)
    {
        float directionX = playerTransform.position.x > transform.position.x ? 1f : -1f;

        return new Vector2(directionX * knockbackX, knockbackY);
    }

    private int GetAdaptedDamage()
    {
        float damageMultiplier = 1f;

        if (useAdaptiveDifficulty && AdaptiveDifficultyManager.Instance != null)
        {
            DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();
            damageMultiplier = settings.enemyDamageMultiplier;
        }

        int adaptedDamage = Mathf.RoundToInt(damageAmount * damageMultiplier);
        return Mathf.Max(1, adaptedDamage);
    }

    private void DestroyBullet()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;
        Destroy(gameObject);
    }

    private void ApplyDifficultySettings()
    {
        adaptedSpeed = speed;
        adaptedLifeTime = lifeTime;

        if (!useAdaptiveDifficulty)
            return;

        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();

        // Reutilizamos el multiplicador de velocidad de enemigos para que las balas
        // también sean más o menos exigentes según la dificultad actual.
        adaptedSpeed = speed * settings.enemySpeedMultiplier;

        // Por defecto no adapto el tiempo de vida para evitar que la bala cambie demasiado su alcance.
        // Si se activa, en dificultades altas también puede recorrer más distancia.
        if (adaptLifeTime)
            adaptedLifeTime = lifeTime * settings.enemySpeedMultiplier;

        adaptedSpeed = Mathf.Max(0.1f, adaptedSpeed);
        adaptedLifeTime = Mathf.Max(0.1f, adaptedLifeTime);
    }

    public void SetDirection(BulletDirection newDirection)
    {
        bulletDirection = newDirection;

        if (Application.isPlaying)
            MoveBullet();
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0.1f, newSpeed);
        ApplyDifficultySettings();

        if (Application.isPlaying)
            MoveBullet();
    }
}