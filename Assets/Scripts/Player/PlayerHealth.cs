using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHearts = 3;
    [SerializeField] private int currentHealth;

    [Header("Invulnerability")]
    [SerializeField] private float invulnerabilityTime = 2f;
    [SerializeField] private float blinkInterval = 0.2f;
    private bool isInvulnerable;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public bool hitReceived;

    public int MaxHealth => maxHearts * 4;
    public int CurrentHealth => currentHealth;
    public bool IsInvulnerable => isInvulnerable;

    public event Action OnHealthChanged;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = MaxHealth;
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

        ApplyKnockback(knockbackForce);

        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvulnerabilityCoroutine());
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        OnHealthChanged?.Invoke();
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
        Debug.Log("El jugador ha muerto");
        Destroy(gameObject);
    }
}