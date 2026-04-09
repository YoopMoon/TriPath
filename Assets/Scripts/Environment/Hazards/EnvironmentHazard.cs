using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float knockbackY = 4f;

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        if (!trigger.transform.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = trigger.transform.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        Vector2 knockbackForce = new Vector2(0f, knockbackY);

        Debug.Log("Player damaged by hazard");
        playerHealth.TakeDamage(damageAmount, knockbackForce);
    }
}