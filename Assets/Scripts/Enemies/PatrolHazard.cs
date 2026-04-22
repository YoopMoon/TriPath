using UnityEngine;

public class PatrolHazard : MonoBehaviour
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
    [SerializeField] private float waitDuration = 0f;
    [SerializeField] private float arrivalThreshold = 0.05f;

    [Header("Impact Settings")]
    [SerializeField] private string floorTag = "Floor";
    [SerializeField] private float impactPauseDuration = 0.5f;
    [SerializeField] private float animationCooldown = 0.1f;

    [Header("Impact Animations")]
    [SerializeField] private string bottomHitAnimationName = "BottomHit";
    [SerializeField] private string leftHitAnimationName = "LeftHit";
    [SerializeField] private string rightHitAnimationName = "RightHit";
    [SerializeField] private string topHitAnimationName = "TopHit";

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

        ApplyMovementConstraints();
    }

    private void Start()
    {
        waitTimer = waitDuration;
    }

    private void FixedUpdate()
    {
        if (rb == null || patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (isPausedByImpact)
        {
            UpdateImpactPause();
            return;
        }

        MoveTowardsCurrentPoint();
    }

    private void ApplyMovementConstraints()
    {
        if (rb == null)
            return;

        switch (movementMode)
        {
            case MovementMode.Vertical:
                // En vertical solo debe moverse en Y.
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                break;

            case MovementMode.Horizontal:
                // En horizontal solo debe moverse en X.
                rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                break;

            case MovementMode.Free:
            default:
                // En movimiento libre permitimos X e Y, pero bloqueamos la rotación.
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

        currentFallSpeed = 0f;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
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
            // Subida controlada con velocidad constante.
            currentFallSpeed = 0f;

            newY = Mathf.MoveTowards(
                currentPosition.y,
                targetPosition.y,
                upwardSpeed * Time.fixedDeltaTime
            );
        }
        else if (targetPosition.y < currentPosition.y)
        {
            // Caída con aceleración progresiva para que no parezca una simple
            // bajada a velocidad fija.
            currentFallSpeed += fallAcceleration * Time.fixedDeltaTime;
            currentFallSpeed = Mathf.Min(currentFallSpeed, maxFallSpeed);

            newY = currentPosition.y - currentFallSpeed * Time.fixedDeltaTime;

            if (newY < targetPosition.y)
                newY = targetPosition.y;
        }

        return new Vector2(currentPosition.x, newY);
    }

    private void MoveToNextPoint()
    {
        currentPointIndex++;

        if (currentPointIndex >= patrolPoints.Length)
            currentPointIndex = 0;

        waitTimer = waitDuration;
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

        // Tras impactar, el hazard se detiene un instante antes de continuar
        // hacia el siguiente punto de patrulla.
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

        // Evitamos reiniciar una animación de impacto si ya se está reproduciendo.
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
}