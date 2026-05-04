using UnityEngine;

public class PatrolHRockHeadMovementazard : MonoBehaviour
{
    public enum MovementMode
    {
        Horizontal,
        Vertical,
        Free
    }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement Settings")]
    [SerializeField] private MovementMode movementMode = MovementMode.Horizontal;
    [SerializeField] private Transform[] patrolPoints;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float upwardSpeed = 1f;
    [SerializeField] private float fallAcceleration = 10f;
    [SerializeField] private float maxFallSpeed = 7f;

    [SerializeField] private float waitDuration = 2f;
    [SerializeField] private float arrivalThreshold = 0.05f;

    [Header("Vertical Movement")]
    [SerializeField] private bool waitOnlyAtTopWhenVertical = true;

    [Header("Impact Settings")]
    [SerializeField] private string floorTag = "Floor";
    [SerializeField] private float impactPauseDuration = 0.5f;
    [SerializeField] private float animationCooldown = 0.1f;

    [Header("Impact Animations")]
    [SerializeField] private string bottomHitAnimationName = "BottomHit";
    [SerializeField] private string leftHitAnimationName = "LeftHit";
    [SerializeField] private string rightHitAnimationName = "RightHit";
    [SerializeField] private string topHitAnimationName = "TopHit";

    [Header("Audio")]
    [SerializeField] private AudioClip impactClip;
    [SerializeField] private float impactVolume = 0.5f;

    private int currentPointIndex;
    private float waitTimer;
    private float currentFallSpeed;
    private float lastImpactAnimationTime;
    private float impactPauseTimer;
    private bool isPausedByImpact;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        ApplyDifficultySettings();
        ApplyMovementConstraints();
    }

    private void Start()
    {
        waitTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (rb == null || patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (patrolPoints[currentPointIndex] == null)
            return;

        if (isPausedByImpact)
        {
            UpdateImpactPause();
            return;
        }

        MoveTowardsCurrentPoint();
    }

    private void ApplyDifficultySettings()
    {
        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();

        float speedMultiplier = settings.enemySpeedMultiplier;
        float waitMultiplier = settings.enemyWaitDurationMultiplier;

        moveSpeed = Mathf.Max(0f, moveSpeed * speedMultiplier);
        upwardSpeed = Mathf.Max(0f, upwardSpeed * speedMultiplier);

        // Para hazards verticales que caen, también hacemos que la caída escale con la dificultad.
        fallAcceleration = Mathf.Max(0f, fallAcceleration * speedMultiplier);
        maxFallSpeed = Mathf.Max(0f, maxFallSpeed * speedMultiplier);

        // En dificultades más altas normalmente este multiplicador debería ser menor,
        // para que espere menos tiempo antes de moverse de nuevo.
        waitDuration = Mathf.Max(0f, waitDuration * waitMultiplier);
        impactPauseDuration = Mathf.Max(0f, impactPauseDuration * waitMultiplier);
    }

    private void ApplyMovementConstraints()
    {
        if (rb == null)
            return;

        switch (movementMode)
        {
            case MovementMode.Vertical:
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                break;

            case MovementMode.Horizontal:
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                break;

            case MovementMode.Free:
            default:
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                break;
        }
    }

    private void UpdateImpactPause()
    {
        impactPauseTimer -= Time.fixedDeltaTime;

        if (impactPauseTimer > 0f)
            return;

        isPausedByImpact = false;
        MoveToNextPoint();
    }

    private void MoveTowardsCurrentPoint()
    {
        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = GetFilteredTargetPosition(patrolPoints[currentPointIndex].position);
        Vector2 newPosition = currentPosition;

        switch (movementMode)
        {
            case MovementMode.Horizontal:
                newPosition = MoveHorizontally(currentPosition, targetPosition);
                break;

            case MovementMode.Vertical:
                newPosition = MoveVertically(currentPosition, targetPosition);
                break;

            case MovementMode.Free:
            default:
                newPosition = Vector2.MoveTowards(
                    currentPosition,
                    targetPosition,
                    moveSpeed * Time.fixedDeltaTime
                );
                break;
        }

        rb.MovePosition(newPosition);

        float distanceToTarget = Vector2.Distance(newPosition, targetPosition);

        if (distanceToTarget > arrivalThreshold)
            return;

        rb.MovePosition(targetPosition);
        currentFallSpeed = 0f;

        if (ShouldWaitAtCurrentPoint(targetPosition))
        {
            if (waitTimer <= 0f)
                waitTimer = waitDuration;

            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer > 0f)
                return;
        }

        MoveToNextPoint();
    }

    private Vector2 MoveHorizontally(Vector2 currentPosition, Vector2 targetPosition)
    {
        float newX = Mathf.MoveTowards(
            currentPosition.x,
            targetPosition.x,
            moveSpeed * Time.fixedDeltaTime
        );

        return new Vector2(newX, currentPosition.y);
    }

    private Vector2 MoveVertically(Vector2 currentPosition, Vector2 targetPosition)
    {
        float newY = currentPosition.y;

        if (targetPosition.y > currentPosition.y)
        {
            currentFallSpeed = 0f;

            newY = Mathf.MoveTowards(
                currentPosition.y,
                targetPosition.y,
                upwardSpeed * Time.fixedDeltaTime
            );
        }
        else if (targetPosition.y < currentPosition.y)
        {
            currentFallSpeed += fallAcceleration * Time.fixedDeltaTime;
            currentFallSpeed = Mathf.Min(currentFallSpeed, maxFallSpeed);

            newY = currentPosition.y - currentFallSpeed * Time.fixedDeltaTime;

            if (newY < targetPosition.y)
                newY = targetPosition.y;
        }

        return new Vector2(currentPosition.x, newY);
    }

    private bool ShouldWaitAtCurrentPoint(Vector2 targetPosition)
    {
        if (waitDuration <= 0f)
            return false;

        if (movementMode != MovementMode.Vertical)
            return true;

        if (!waitOnlyAtTopWhenVertical)
            return true;

        return IsTopPoint(targetPosition);
    }

    private bool IsTopPoint(Vector2 targetPosition)
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return false;

        float highestY = patrolPoints[0].position.y;

        for (int i = 1; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            if (patrolPoints[i].position.y > highestY)
                highestY = patrolPoints[i].position.y;
        }

        return Mathf.Abs(targetPosition.y - highestY) <= arrivalThreshold;
    }

    private void MoveToNextPoint()
    {
        currentPointIndex++;

        if (currentPointIndex >= patrolPoints.Length)
            currentPointIndex = 0;

        waitTimer = 0f;
        currentFallSpeed = 0f;
    }

    private Vector2 GetFilteredTargetPosition(Vector2 originalTarget)
    {
        Vector2 currentPosition = rb != null ? rb.position : (Vector2)transform.position;

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
        if (!collision.gameObject.CompareTag(floorTag))
            return;

        if (Time.time - lastImpactAnimationTime < animationCooldown)
            return;

        if (collision.contactCount == 0)
            return;

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;

        PlayImpactAnimation(normal);

        lastImpactAnimationTime = Time.time;
        currentFallSpeed = 0f;

        isPausedByImpact = true;
        impactPauseTimer = impactPauseDuration;
    }

    private void PlayImpactAnimation(Vector2 normal)
    {
        if (animator == null)
            return;

        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
        {
            if (normal.x > 0f)
                PlayAnimationOnce(leftHitAnimationName);
            else
                PlayAnimationOnce(rightHitAnimationName);
        }
        else
        {
            if (normal.y > 0f)
                PlayAnimationOnce(bottomHitAnimationName);
            else
                PlayAnimationOnce(topHitAnimationName);
        }
    }

    private void PlayAnimationOnce(string animationName)
    {
        if (animator == null || string.IsNullOrEmpty(animationName))
            return;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

        PlaySFXImpact();

        if (currentState.IsName(animationName) && currentState.normalizedTime < 1f)
            return;

        animator.Play(animationName, 0, 0f);
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
    }

    private void OnValidate()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        ApplyMovementConstraints();
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Gizmos.color = Color.red;

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

    private void PlaySFXImpact()
    {
        if (impactClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(impactClip, impactVolume);
    }
}