using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHearts = 3;
    [SerializeField] private int currentHealth;

    [Header("Invulnerability")]
    [SerializeField] private float invulnerabilityTime = 1.5f;
    [SerializeField] private float blinkInterval = 0.1f;
    private bool isInvulnerable;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Death Animation")]
    [SerializeField] private float deathJumpForce = 1.6f;
    [SerializeField] private float deathGravity = 14f;
    [SerializeField] private float deathDelayBeforeGameOver = 2f;
    [SerializeField] private float deathRotationSpeed = 180f;
    [SerializeField] private bool rotateOnDeath = true;
    [SerializeField] private bool disableAnimatorOnDeath = true;

    [Header("Audio")]
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private float damageVolume = 1f;
    [SerializeField] private AudioClip healClip;
    [SerializeField] private float healVolume = 1f;

    public bool hitReceived;

    public int MaxHealth => maxHearts * 4;
    public int CurrentHealth => currentHealth;
    public bool IsInvulnerable => isInvulnerable;

    public event Action OnHealthChanged;
    public event Action OnDamageTaken;
    public event Action OnHealed;

    private bool isDead;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        InitializeHealth();
    }

    private void InitializeHealth()
    {
        if (SceneTransitionManager.instance != null && SceneTransitionManager.instance.HasSavedHealth())
        {
            currentHealth = SceneTransitionManager.instance.GetSavedHealth();
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

            Debug.Log($"[PlayerHealth] Vida restaurada: {currentHealth}/{MaxHealth}");
        }
        else
        {
            currentHealth = MaxHealth;

            Debug.Log($"[PlayerHealth] Vida inicializada al máximo: {currentHealth}/{MaxHealth}");
        }

        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, Vector2.zero);
    }

    public void TakeDamage(int amount, Vector2 knockbackForce)
    {
        if (isDead)
            return;

        if (isInvulnerable)
            return;

        hitReceived = true;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        if (damageClip != null && SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(damageClip, damageVolume);

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.RegisterDamageTaken(amount);

        ApplyKnockback(knockbackForce);

        OnHealthChanged?.Invoke();
        OnDamageTaken?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvulnerabilityCoroutine());
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        int previousHealth = currentHealth;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        if (currentHealth <= previousHealth)
            return;

        if (healClip != null && SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(healClip, healVolume);

        OnHealthChanged?.Invoke();
        OnHealed?.Invoke();
    }

    private void ApplyKnockback(Vector2 knockbackForce)
    {
        if (rb == null || knockbackForce == Vector2.zero)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackForce, ForceMode2D.Impulse);
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float elapsedTime = 0f;

        while (elapsedTime < invulnerabilityTime)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.RegisterDeath();

        StopAllCoroutines();

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        DisablePlayerInteractionOnDeath();

        if (animator != null && disableAnimatorOnDeath)
            animator.enabled = false;

        StartCoroutine(DeathFallCoroutine());
    }

    private void DisablePlayerInteractionOnDeath()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D collider in colliders)
            collider.enabled = false;

        PlayerController playerController = GetComponent<PlayerController>();

        if (playerController != null)
            playerController.enabled = false;
    }

    private IEnumerator DeathFallCoroutine()
    {
        Transform visualTransform = spriteRenderer != null ? spriteRenderer.transform : transform;

        float verticalVelocity = deathJumpForce;
        float timer = 0f;

        while (timer < deathDelayBeforeGameOver)
        {
            timer += Time.deltaTime;

            verticalVelocity -= deathGravity * Time.deltaTime;
            visualTransform.position += Vector3.up * verticalVelocity * Time.deltaTime;

            if (rotateOnDeath)
                visualTransform.Rotate(0f, 0f, deathRotationSpeed * Time.deltaTime);

            yield return null;
        }

        ShowGameOver();
    }

    private void ShowGameOver()
    {
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.HandlePlayerDeath();
            return;
        }

        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.ClearPlayerHealth();

        Destroy(gameObject);
    }

    public void SaveCurrentHealth()
    {
        if (SceneTransitionManager.instance == null)
            return;

        SceneTransitionManager.instance.SetPlayerHealth(currentHealth);

        Debug.Log($"[PlayerHealth] Vida guardada al salir de escena: {currentHealth}/{MaxHealth}");
    }

    public void SetMaxHearts(int hearts)
    {
        maxHearts = Mathf.Max(1, hearts);

        if (SceneTransitionManager.instance != null && SceneTransitionManager.instance.HasSavedHealth())
        {
            currentHealth = SceneTransitionManager.instance.GetSavedHealth();
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        }
        else
        {
            currentHealth = MaxHealth;
        }

        OnHealthChanged?.Invoke();

        Debug.Log($"[PlayerHealth] SetMaxHearts -> Vida actual: {currentHealth}/{MaxHealth}");
    }
}