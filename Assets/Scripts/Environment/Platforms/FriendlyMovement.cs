using System.Collections.Generic;
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

    [Header("Passenger Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float detachVerticalVelocity = 0.1f;

    private static readonly Dictionary<Transform, Transform> originalParentsByPassenger = new();
    private static readonly Dictionary<Transform, FriendlyMovement> currentPlatformByPassenger = new();

    private int currentPointIndex;
    private float waitTimer;
    private bool isWaiting;

    private Transform playerOnPlatform;
    private Rigidbody2D playerRb;

    private bool isQuittingOrDisabling;

    private void Awake()
    {
        ApplyDifficultySettings();
    }

    private void Start()
    {
        waitTimer = 0f;
        isWaiting = false;
    }

    private void Update()
    {
        MoveTowardsCurrentPoint();
        UpdateAnimationState();
        CheckPassengerStillValid();
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
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (patrolPoints[currentPointIndex] == null)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer > 0f)
                return;

            isWaiting = false;
            MoveToNextPoint();
        }

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = GetFilteredTargetPosition(patrolPoints[currentPointIndex].position);

        Vector2 newPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        transform.position = newPosition;

        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        if (distanceToTarget > arrivalThreshold)
            return;

        transform.position = targetPosition;

        if (waitDuration > 0f)
        {
            isWaiting = true;
            waitTimer = waitDuration;
            return;
        }

        MoveToNextPoint();
    }

    private void MoveToNextPoint()
    {
        currentPointIndex++;

        if (currentPointIndex >= patrolPoints.Length)
            currentPointIndex = 0;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryAttachPlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryAttachPlayer(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        TryDetachPlayer(collision);
    }

    private void TryAttachPlayer(Collision2D collision)
    {
        if (!collision.transform.CompareTag(playerTag))
            return;

        if (!IsPlayerOnTop(collision))
            return;

        if (!collision.gameObject.TryGetComponent(out Rigidbody2D detectedRb))
            return;

        Transform passenger = collision.transform;

        if (playerOnPlatform == passenger)
            return;

        if (currentPlatformByPassenger.TryGetValue(passenger, out FriendlyMovement previousPlatform))
        {
            if (previousPlatform != null && previousPlatform != this)
                previousPlatform.ForceReleasePassenger(passenger);
        }

        if (!originalParentsByPassenger.ContainsKey(passenger))
            originalParentsByPassenger.Add(passenger, passenger.parent);

        playerOnPlatform = passenger;
        playerRb = detectedRb;

        currentPlatformByPassenger[passenger] = this;

        passenger.SetParent(transform, true);
    }

    private void TryDetachPlayer(Collision2D collision)
    {
        if (playerOnPlatform == null)
            return;

        if (collision.transform != playerOnPlatform)
            return;

        DetachPlayer();
    }

    private void ForceReleasePassenger(Transform passenger)
    {
        if (playerOnPlatform != passenger)
            return;

        playerOnPlatform = null;
        playerRb = null;
    }

    private void DetachPlayer()
    {
        if (playerOnPlatform == null)
            return;

        Transform passenger = playerOnPlatform;

        if (!isQuittingOrDisabling)
        {
            if (originalParentsByPassenger.TryGetValue(passenger, out Transform originalParent))
                passenger.SetParent(originalParent, true);
            else
                passenger.SetParent(null, true);
        }

        if (currentPlatformByPassenger.TryGetValue(passenger, out FriendlyMovement currentPlatform) &&
            currentPlatform == this)
        {
            currentPlatformByPassenger.Remove(passenger);
            originalParentsByPassenger.Remove(passenger);
        }

        playerOnPlatform = null;
        playerRb = null;
    }

    private bool IsPlayerOnTop(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
                return true;
        }

        return false;
    }

    private void CheckPassengerStillValid()
    {
        if (playerOnPlatform == null || playerRb == null)
            return;

        if (playerRb.linearVelocity.y > detachVerticalVelocity)
            DetachPlayer();
    }

    private void UpdateAnimationState()
    {
        if (animator == null)
            return;

        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (patrolPoints[currentPointIndex] == null)
            return;

        Vector2 targetPosition = GetFilteredTargetPosition(patrolPoints[currentPointIndex].position);
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        bool isIdle = distanceToTarget <= arrivalThreshold && isWaiting;

        // animator.SetBool("Idle", isIdle);
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }

    private void OnDisable()
    {
        isQuittingOrDisabling = true;

        if (playerOnPlatform != null)
            ForceCleanupPassenger(playerOnPlatform);

        playerOnPlatform = null;
        playerRb = null;
    }

    private void OnDestroy()
    {
        isQuittingOrDisabling = true;

        if (playerOnPlatform != null)
            ForceCleanupPassenger(playerOnPlatform);

        playerOnPlatform = null;
        playerRb = null;
    }

    private void ForceCleanupPassenger(Transform passenger)
    {
        if (passenger == null)
            return;

        if (currentPlatformByPassenger.TryGetValue(passenger, out FriendlyMovement currentPlatform) &&
            currentPlatform == this)
        {
            currentPlatformByPassenger.Remove(passenger);
            originalParentsByPassenger.Remove(passenger);
        }
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