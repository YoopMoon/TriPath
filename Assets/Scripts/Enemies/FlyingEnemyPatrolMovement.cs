using System.Collections;
using UnityEngine;

public class FlyingEnemyPatrolMovement : MonoBehaviour
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
    [SerializeField] private MovementMode movementMode = MovementMode.Free;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Idle Sequence Points")]
    [SerializeField] private int[] idleSequencePointIndexes;

    [Header("Animator Parameters")]
    [SerializeField] private string idleParameterName = "Idle";
    [SerializeField] private string foldWingsTriggerName = "FoldWings";
    [SerializeField] private string unfoldWingsTriggerName = "UnfoldWings";

    [Header("Animator State Names")]
    [SerializeField] private string foldWingsStateName = "CeilingIn";
    [SerializeField] private string unfoldWingsStateName = "CeilingOut";
    [SerializeField] private float animationStateFallbackTimeout = 1.5f;

    [Header("Audio")]
    [SerializeField] private bool useMovementLoopSFX;
    [SerializeField] private AudioClip movementLoopClip;
    [SerializeField] private float movementLoopVolume = 1f;
    [SerializeField] private AudioSource movementLoopSource;

    private BoxCollider2D topBoxCollider;
    private CapsuleCollider2D topCapsuleCollider;
    private CircleCollider2D topCircleCollider;

    private int currentPointIndex;
    private int currentHealth;
    private float waitTimer;
    private float initialColliderOffsetX;

    private bool hasIdleParameter;
    private bool hasFoldWingsTrigger;
    private bool hasUnfoldWingsTrigger;

    private bool isDead;
    private bool isWaitingAtPoint;
    private bool isPlayingIdleSequence;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        CacheAnimatorParameters();
        CacheTopDamageCollider();

        ApplyDifficultySettings();
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        SFXManager.OnMasterSfxVolumeChanged += HandleMasterSfxVolumeChanged;
        ApplyMovementLoopVolume();
    }

    private void Start()
    {
        waitTimer = waitDuration;
        StartMovementLoopSFX();
    }

    private void Update()
    {
        if (isDead || isPlayingIdleSequence || patrolPoints == null || patrolPoints.Length == 0)
            return;

        MoveTowardsCurrentPoint();
    }

    private void CacheAnimatorParameters()
    {
        if (animator == null)
            return;

        hasIdleParameter = HasBoolParameter(idleParameterName);
        hasFoldWingsTrigger = HasTriggerParameter(foldWingsTriggerName);
        hasUnfoldWingsTrigger = HasTriggerParameter(unfoldWingsTriggerName);
    }

    private void CacheTopDamageCollider()
    {
        if (topDamageCollider == null)
            return;

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

        if (targetPoint == null)
            return;

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = GetFilteredTargetPosition(targetPoint.position);

        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        if (distanceToTarget > arrivalThreshold)
        {
            isWaitingAtPoint = false;

            transform.position = Vector2.MoveTowards(
                currentPosition,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            UpdateSpriteDirection(targetPosition);
            return;
        }

        isWaitingAtPoint = true;

        if (ShouldPlayIdleSequenceAtCurrentPoint())
        {
            StartCoroutine(IdleSequenceCoroutine());
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        GoToNextPatrolPoint();
    }

    private bool ShouldPlayIdleSequenceAtCurrentPoint()
    {
        if (idleSequencePointIndexes == null || idleSequencePointIndexes.Length == 0)
            return false;

        if (isPlayingIdleSequence)
            return false;

        for (int i = 0; i < idleSequencePointIndexes.Length; i++)
        {
            if (idleSequencePointIndexes[i] == currentPointIndex)
                return true;
        }

        return false;
    }

    private IEnumerator IdleSequenceCoroutine()
    {
        isPlayingIdleSequence = true;
        isWaitingAtPoint = true;

        SetIdleAnimation(false);

        if (animator != null && hasFoldWingsTrigger)
        {
            animator.ResetTrigger(unfoldWingsTriggerName);
            animator.SetTrigger(foldWingsTriggerName);

            yield return WaitForAnimatorStateToFinish(
                foldWingsStateName,
                animationStateFallbackTimeout
            );
        }

        if (isDead)
            yield break;

        SetIdleAnimation(true);

        if (waitDuration > 0f)
            yield return new WaitForSeconds(waitDuration);

        if (isDead)
            yield break;

        SetIdleAnimation(false);

        if (animator != null && hasUnfoldWingsTrigger)
        {
            animator.ResetTrigger(foldWingsTriggerName);
            animator.SetTrigger(unfoldWingsTriggerName);

            yield return WaitForAnimatorStateToFinish(
                unfoldWingsStateName,
                animationStateFallbackTimeout
            );
        }

        if (isDead)
            yield break;

        GoToNextPatrolPoint();

        isWaitingAtPoint = false;
        isPlayingIdleSequence = false;
    }

    private IEnumerator WaitForAnimatorStateToFinish(string stateName, float timeout)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
            yield break;

        bool hasEnteredState = false;
        float timer = 0f;

        while (!isDead && timer < timeout)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);

            bool currentMatches = AnimatorStateMatches(currentState, stateName);
            bool nextMatches = animator.IsInTransition(0) && AnimatorStateMatches(nextState, stateName);

            if (currentMatches || nextMatches)
            {
                hasEnteredState = true;
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (!hasEnteredState)
            yield break;

        timer = 0f;

        while (!isDead && timer < timeout)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            bool isInExpectedState = AnimatorStateMatches(currentState, stateName);
            bool isTransitioning = animator.IsInTransition(0);

            if (!isInExpectedState && !isTransitioning)
                yield break;

            if (isInExpectedState && currentState.normalizedTime >= 1f && !isTransitioning)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private bool AnimatorStateMatches(AnimatorStateInfo stateInfo, string stateName)
    {
        return stateInfo.IsName(stateName) ||
               stateInfo.shortNameHash == Animator.StringToHash(stateName);
    }

    private void GoToNextPatrolPoint()
    {
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
        if (!spriteRendererFlipEnabled || spriteRenderer == null)
            return;

        float horizontalDirection = targetPosition.x - transform.position.x;

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

    private void SetIdleAnimation(bool isIdle)
    {
        if (animator == null || !hasIdleParameter)
            return;

        animator.SetBool(idleParameterName, isIdle);
    }

    private bool HasBoolParameter(string parameterName)
    {
        if (animator == null)
            return false;

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

    private bool HasTriggerParameter(string parameterName)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }

    private void StartMovementLoopSFX()
    {
        if (!useMovementLoopSFX || movementLoopClip == null)
            return;

        if (movementLoopSource == null)
            movementLoopSource = GetComponent<AudioSource>();

        if (movementLoopSource == null)
            movementLoopSource = gameObject.AddComponent<AudioSource>();

        movementLoopSource.clip = movementLoopClip;
        ApplyMovementLoopVolume();
        movementLoopSource.loop = true;
        movementLoopSource.playOnAwake = false;

        if (!movementLoopSource.isPlaying)
            movementLoopSource.Play();
    }

    private void StopMovementLoopSFX()
    {
        if (movementLoopSource == null)
            return;

        movementLoopSource.Stop();
    }

    private void ApplyMovementLoopVolume()
    {
        if (movementLoopSource == null)
            return;

        movementLoopSource.volume = movementLoopVolume * SFXManager.MasterSfxVolume;
    }

    private void HandleMasterSfxVolumeChanged(float volume)
    {
        ApplyMovementLoopVolume();
    }

    private void OnDisable()
    {
        SFXManager.OnMasterSfxVolumeChanged -= HandleMasterSfxVolumeChanged;
        StopMovementLoopSFX();
        SetIdleAnimation(false);
        isPlayingIdleSequence = false;
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
        StopMovementLoopSFX();
        SetIdleAnimation(false);
        isPlayingIdleSequence = false;

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Gizmos.color = Color.cyan;

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
