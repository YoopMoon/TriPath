using UnityEngine;

public class SpikeHead : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float knockbackX = 4f;
    [SerializeField] private float knockbackY = 2.5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = collision.transform.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        float directionX = collision.transform.position.x > transform.position.x ? 1f : -1f;
        Vector2 knockbackForce = new Vector2(directionX * knockbackX, knockbackY);

        Debug.Log("Player damaged");
        playerHealth.TakeDamage(damageAmount, knockbackForce);
    }
}