using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float knockbackY = 4f;

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        TryDamagePlayer(trigger);
    }

    private void OnTriggerStay2D(Collider2D trigger)
    {
        TryDamagePlayer(trigger);
    }

    private void TryDamagePlayer(Collider2D trigger)
    {
        if (!trigger.transform.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = trigger.transform.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        Vector2 knockbackForce = new Vector2(0f, knockbackY);
        int finalDamage = GetAdaptedDamage();

        playerHealth.TakeDamage(finalDamage, knockbackForce);
    }

    private int GetAdaptedDamage()
    {
        float damageMultiplier = 1f;

        if (AdaptiveDifficultyManager.Instance != null)
        {
            DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();
            damageMultiplier = settings.environmentDamageMultiplier;
        }

        int adaptedDamage = Mathf.RoundToInt(damageAmount * damageMultiplier);
        return Mathf.Max(1, adaptedDamage);
    }
}