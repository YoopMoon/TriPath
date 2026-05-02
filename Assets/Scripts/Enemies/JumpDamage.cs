using UnityEngine;

public class JumpDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private GameObject destroyParticle;

    [Header("Enemy Settings")]
    [SerializeField] private float bounceForce = 2.5f;
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float destroyDelay = 0.2f;
    [SerializeField] private string hitAnimationName = "Hit";

    [Header("Audio")]
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private float damageVolume = 1f;


    private int currentHealth;
    private bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        ApplyDifficultySettings();
        currentHealth = maxHealth;
    }

    private void ApplyDifficultySettings()
    {
        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();
        maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * settings.enemyHealthMultiplier));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (!collision.gameObject.TryGetComponent(out Rigidbody2D playerRb))
            return;

        if (!IsValidStomp(collision, playerRb))
            return;

        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        playerRb.linearVelocity += Vector2.up * bounceForce;

        TakeDamage(1);
    }

    private bool IsValidStomp(Collision2D collision, Rigidbody2D playerRb)
    {
        if (playerRb.linearVelocity.y > 0f)
            return false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
                return true;
        }

        return false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        PlayDamageSound();

        if (currentHealth > 0)
        {
            PlayHitAnimationOnce();
            return;
        }

        Die();
    }

    public void SetMaxHealth(int newMaxHealth, bool restoreCurrentHealth = true)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);

        if (restoreCurrentHealth)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    private void PlayHitAnimationOnce()
    {
        if (animator == null || string.IsNullOrEmpty(hitAnimationName))
            return;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName(hitAnimationName) && currentState.normalizedTime < 1f)
            return;

        animator.Play(hitAnimationName, 0, 0f);
    }

    private void Die()
    {
        isDead = true;

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (destroyParticle != null)
            destroyParticle.SetActive(true);

        Destroy(gameObject, destroyDelay);
    }

    private void PlayDamageSound()
    {
        if (damageClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(damageClip, damageVolume);
    }
}