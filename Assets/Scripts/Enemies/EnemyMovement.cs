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
    [SerializeField] private bool spriteRendererFlipEnabled;
    [SerializeField] private Collider2D topDamageCollider;

    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 1;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float waitDuration = 2f;
    [SerializeField] private float arrivalThreshold = 0.05f;
    [SerializeField] private MovementMode movementMode = MovementMode.Horizontal;
    [SerializeField] private Transform[] patrolPoints;

    private BoxCollider2D topBoxCollider;
    private CapsuleCollider2D topCapsuleCollider;
    private CircleCollider2D topCircleCollider;

    private int currentPointIndex;
    private int currentHealth;
    private float waitTimer;
    private float initialColliderOffsetX;

    private bool hasIdleParameter;
    private bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        if (animator != null)
            hasIdleParameter = HasBoolParameter("Idle");

        if (topDamageCollider != null)
        {
            topBoxCollider = topDamageCollider as BoxCollider2D;
            topCapsuleCollider = topDamageCollider as CapsuleCollider2D;
            topCircleCollider = topDamageCollider as CircleCollider2D;

            if (topBoxCollider != null)
                initialColliderOffsetX = Mathf.Abs(topBoxCollider.offset.x);
            else if (topCapsuleCollider != null)
                initialColliderOffsetX = Mathf.Abs(topCapsuleCollider.offset.x);
            else if (topCircleCollider != null)
                initialColliderOffsetX = Mathf.Abs(topCircleCollider.offset.x);
        }

        ApplyDifficultySettings();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        waitTimer = waitDuration;
    }

    private void Update()
    {
        if (isDead || patrolPoints == null || patrolPoints.Length == 0)
            return;

        MoveTowardsCurrentPoint();
        UpdateAnimationState();
    }

    private void ApplyDifficultySettings()
    {
        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();

        maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * settings.enemyHealthMultiplier));
        moveSpeed = Mathf.Max(0f, moveSpeed * settings.enemySpeedMultiplier);
        waitDuration = Mathf.Max(0f, waitDuration * settings.enemyWaitDurationMultiplier);
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
                return new Vector2(originalTarget.x, currentPosition.y);

            case MovementMode.Vertical:
                return new Vector2(currentPosition.x, originalTarget.y);

            case MovementMode.Free:
            default:
                return originalTarget;
        }
    }

    private void UpdateSpriteDirection(Vector2 targetPosition)
    {
        if (spriteRendererFlipEnabled)
        {
            float horizontalDirection = targetPosition.x - (float)transform.position.x;

            if (horizontalDirection > 0.01f)
            {
                spriteRenderer.flipX = true;
                UpdateTopColliderSide(true);
            }
            else if (horizontalDirection < -0.01f)
            {
                spriteRenderer.flipX = false;
                UpdateTopColliderSide(false);
            }
        }
    }

    private void UpdateTopColliderSide(bool facingRight)
    {
        float targetOffsetX = facingRight ? initialColliderOffsetX : -initialColliderOffsetX;

        if (topBoxCollider != null)
        {
            Vector2 offset = topBoxCollider.offset;
            offset.x = targetOffsetX;
            topBoxCollider.offset = offset;
        }
        else if (topCapsuleCollider != null)
        {
            Vector2 offset = topCapsuleCollider.offset;
            offset.x = targetOffsetX;
            topCapsuleCollider.offset = offset;
        }
        else if (topCircleCollider != null)
        {
            Vector2 offset = topCircleCollider.offset;
            offset.x = targetOffsetX;
            topCircleCollider.offset = offset;
        }
    }

    private void UpdateAnimationState()
    {
        if (animator == null || !hasIdleParameter)
            return;

        Vector2 targetPosition = GetFilteredTargetPosition(patrolPoints[currentPointIndex].position);
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        bool isIdle = distanceToTarget <= arrivalThreshold && waitTimer > 0f;
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
        if (isDead)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
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
            Gizmos.DrawSphere(pointPosition, 0.06f);

            Transform nextPoint = patrolPoints[(i + 1) % patrolPoints.Length];

            if (nextPoint != null)
                Gizmos.DrawLine(pointPosition, nextPoint.position);
        }
    }
}