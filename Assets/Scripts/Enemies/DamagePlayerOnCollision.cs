using UnityEngine;

public class DamagePlayerOnCollision : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private Vector2 knockbackForce = Vector2.zero;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Este objeto solo debe aplicar daño al jugador.
        if (!collision.gameObject.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = collision.transform.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(damageAmount, Vector2.zero);
    }
}