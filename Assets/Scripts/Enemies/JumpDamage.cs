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

    private int currentHealth;
    private bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (!collision.gameObject.TryGetComponent(out Rigidbody2D playerRb))
            return;

        // Rebote del jugador al golpear al enemigo.
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        playerRb.linearVelocity += Vector2.up * bounceForce;

        TakeDamage(1);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

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

        // Si la animación de golpe ya se está reproduciendo, no la reiniciamos.
        // Así evitamos que se encadene o parezca que entra varias veces seguidas
        // por un mismo contacto.
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
}