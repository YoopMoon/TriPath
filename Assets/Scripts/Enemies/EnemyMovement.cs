using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum MovementMode
    {
        Horizontal,
        Vertical,
        Free
    }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 1;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float waitDuration = 2f;
    [SerializeField] private float arrivalThreshold = 0.05f;
    [SerializeField] private MovementMode movementMode = MovementMode.Horizontal;
    [SerializeField] private Transform[] patrolPoints;

    private int currentPointIndex;
    private float waitTimer;
    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        waitTimer = waitDuration;
    }

    private void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        MoveTowardsCurrentPoint();
        UpdateAnimationState();
    }

    private void MoveTowardsCurrentPoint()
    {
        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = GetFilteredTargetPosition(targetPoint.position);

        transform.position = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        if (distanceToTarget > arrivalThreshold)
        {
            UpdateSpriteDirection(targetPosition);
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        currentPointIndex++;

        if (currentPointIndex >= patrolPoints.Length)
            currentPointIndex = 0;

        waitTimer = waitDuration;
    }

    private Vector2 GetFilteredTargetPosition(Vector2 originalTarget)
    {
        Vector2 currentPosition = transform.position;

        switch (movementMode)
        {
            case MovementMode.Horizontal:
                // Solo cambia la X. La Y se mantiene fija.
                return new Vector2(originalTarget.x, currentPosition.y);

            case MovementMode.Vertical:
                // Solo cambia la Y. La X se mantiene fija.
                return new Vector2(currentPosition.x, originalTarget.y);

            case MovementMode.Free:
            default:
                // Permite movimiento completo entre puntos.
                return originalTarget;
        }
    }

    private void UpdateSpriteDirection(Vector2 targetPosition)
    {
        // El flip solo tiene sentido real cuando hay desplazamiento horizontal.
        // En enemigos que patrullan en vertical, esta parte no altera la orientación.
        float horizontalDirection = targetPosition.x - (float)transform.position.x;

        if (horizontalDirection > 0.01f)
            spriteRenderer.flipX = true;
        else if (horizontalDirection < -0.01f)
            spriteRenderer.flipX = false;
    }

    private void UpdateAnimationState()
    {
        if (animator == null || patrolPoints == null || patrolPoints.Length == 0)
            return;

        Vector2 targetPosition = GetFilteredTargetPosition(patrolPoints[currentPointIndex].position);
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        // Si ya ha alcanzado el punto actual y sigue esperando antes de pasar al siguiente,
        // el enemigo se considera en reposo.
        bool isIdle = distanceToTarget <= arrivalThreshold && waitTimer > 0f;

        // No todos los enemigos usan una animación Idle. Antes de intentar actualizar
        // el parámetro, comprobamos que exista para evitar warnings en consola.
        if (HasBoolParameter("Idle"))
            animator.SetBool("Idle", isIdle);
    }

    private bool HasBoolParameter(string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
        }

        return false;
    }

    public void SetMaxHealth(int newMaxHealth, bool restoreCurrentHealth = true)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);

        if (restoreCurrentHealth)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            Vector2 pointPosition = patrolPoints[i].position;
            Gizmos.DrawSphere(pointPosition, 0.08f);

            Transform nextPoint = patrolPoints[(i + 1) % patrolPoints.Length];

            if (nextPoint != null)
                Gizmos.DrawLine(pointPosition, nextPoint.position);
        }
    }
}