using UnityEngine;

public class VerticalMovingPlatform : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform bottomPoint;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float arrivalThreshold = 0.02f;

    private Rigidbody2D rb;
    private Vector2 currentTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            enabled = false;
            return;
        }

        if (topPoint == null || bottomPoint == null)
        {
            enabled = false;
            return;
        }

        if (rb.bodyType != RigidbodyType2D.Kinematic)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        rb.gravityScale = 0f;

        float distanceToTop = Vector2.Distance(rb.position, topPoint.position);
        float distanceToBottom = Vector2.Distance(rb.position, bottomPoint.position);

        currentTarget = distanceToTop <= distanceToBottom
            ? (Vector2)bottomPoint.position
            : (Vector2)topPoint.position;
    }

    private void FixedUpdate()
    {
        if (Vector2.Distance(topPoint.position, bottomPoint.position) < 0.01f)
        {
            return;
        }

        Vector2 newPosition = Vector2.MoveTowards(rb.position, currentTarget, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if (Vector2.Distance(rb.position, currentTarget) <= arrivalThreshold)
        {
            currentTarget = currentTarget == (Vector2)topPoint.position
                ? (Vector2)bottomPoint.position
                : (Vector2)topPoint.position;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (topPoint == null || bottomPoint == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(topPoint.position, bottomPoint.position);
        Gizmos.DrawSphere(topPoint.position, 0.08f);
        Gizmos.DrawSphere(bottomPoint.position, 0.08f);
    }
}