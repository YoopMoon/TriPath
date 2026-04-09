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
    [SerializeField] private float blinkInterval = 0.1f;
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
        // Si existe un valor de vida guardado en el manager persistente,
        // lo restauramos al entrar en una nueva escena.
        // Si no existe todavía, inicializamos la vida al máximo y la guardamos.
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

        // Guardamos la vida actual para que se mantenga al cambiar de escena.
        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.SetPlayerHealth(currentHealth);

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

        // También actualizamos la vida persistente cuando el jugador se cura.
        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.SetPlayerHealth(currentHealth);

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

        // Reiniciamos la vida guardada para que, si reaparece o vuelve a empezar,
        // no se conserve una vida a 0 entre escenas.
        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.ClearPlayerHealth();

        Destroy(gameObject);
    }
}