using UnityEngine;

public class FriendlyMovement : MonoBehaviour
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

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float waitDuration = 2f;
    [SerializeField] private float arrivalThreshold = 0.05f;
    [SerializeField] private MovementMode movementMode = MovementMode.Horizontal;
    [SerializeField] private Transform[] patrolPoints;

    private int currentPointIndex;
    private float waitTimer;


    private void Awake()
    {
        ApplyDifficultySettings();
    }

    private void Start()
    {
        waitTimer = waitDuration;
    }

    private void Update()
    {
        MoveTowardsCurrentPoint();
        UpdateAnimationState();
    }

    private void ApplyDifficultySettings()
    {
        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();

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

    private void UpdateAnimationState()
    {
        if (animator == null)
            return;

        Vector2 targetPosition = GetFilteredTargetPosition(patrolPoints[currentPointIndex].position);
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        bool isIdle = distanceToTarget <= arrivalThreshold && waitTimer > 0f;
        //animator.SetBool("Idle", isIdle);
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
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