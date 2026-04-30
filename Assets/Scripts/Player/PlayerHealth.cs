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

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (SceneTransitionManager.instance != null && SceneTransitionManager.instance.HasSavedHealth())
        {
            currentHealth = SceneTransitionManager.instance.GetSavedHealth();
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        }
        else
        {
            currentHealth = MaxHealth;

            if (SceneTransitionManager.instance != null)
                SceneTransitionManager.instance.SetPlayerHealth(currentHealth);
        }

        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, Vector2.zero);
    }

    public void TakeDamage(int amount, Vector2 knockbackForce)
    {
        if (isInvulnerable)
            return;

        hitReceived = true;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        if (damageClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(damageClip, damageVolume);

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.RegisterDamageTaken(amount);

        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.SetPlayerHealth(currentHealth);

        ApplyKnockback(knockbackForce);

        OnHealthChanged?.Invoke();
        OnDamageTaken?.Invoke();

        if (currentHealth <= 0)
        {
            if (LevelMetrics.Instance != null)
                LevelMetrics.Instance.RegisterDeath();

            Die();
            return;
        }

        StartCoroutine(InvulnerabilityCoroutine());
    }

    public void Heal(int amount)
    {
        int previousHealth = currentHealth;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        if (currentHealth <= previousHealth)
            return;

        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.SetPlayerHealth(currentHealth);

        if (healClip == null)
            return;

        if (SFXManager.Instance != null)
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
        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.RegisterDeath();

        DisablePlayerOnDeath();

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.HandlePlayerDeath();
            return;
        }

        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.ClearPlayerHealth();

        Destroy(gameObject);
    }

    private void DisablePlayerOnDeath()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
            playerCollider.enabled = false;

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    public void SetMaxHearts(int hearts)
    {
        maxHearts = hearts;
        currentHealth = MaxHealth;
        OnHealthChanged?.Invoke();
    }
}