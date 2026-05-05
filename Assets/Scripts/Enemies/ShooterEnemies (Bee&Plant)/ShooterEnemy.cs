using System.Collections;
using UnityEngine;

public class ShooterEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float waitTimeToAttack = 3f;
    [SerializeField] private float launchDelay = 0.5f;

    [Header("Attack Sequence")]
    [SerializeField] private int bulletsPerSequence = 1;
    [SerializeField] private float timeBetweenBullets = 0.4f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform launchSpawnPoint;

    [Header("Adaptive Difficulty")]
    [SerializeField] private bool useAdaptiveDifficulty = true;

    // Límite de seguridad para que en dificultad alta no dispare de forma absurda.
    [SerializeField] private float minimumWaitTimeToAttack = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioClip bulletClip;
    [SerializeField] private float bulletVolume = 0.5f;

    private float waitedTime;
    private float adaptedWaitTimeToAttack;
    private bool isAttacking;
    private bool isDead;
    private Coroutine attackCoroutine;

    private void Awake()
    {
        ApplyDifficultySettings();
    }

    private void Start()
    {
        waitedTime = adaptedWaitTimeToAttack;
    }

    private void Update()
    {
        if (isDead)
            return;

        if (isAttacking)
            return;

        if (waitedTime <= 0f)
        {
            attackCoroutine = StartCoroutine(AttackSequenceCoroutine());
        }
        else
        {
            waitedTime -= Time.deltaTime;
        }
    }

    private IEnumerator AttackSequenceCoroutine()
    {
        isAttacking = true;

        int safeBulletsPerSequence = Mathf.Max(1, bulletsPerSequence);

        for (int i = 0; i < safeBulletsPerSequence; i++)
        {
            if (isDead)
                yield break;

            if (animator != null && animator.enabled)
                animator.Play("Attack", 0, 0f);

            yield return new WaitForSeconds(launchDelay);

            if (isDead)
                yield break;

            LaunchBullet();
            PlayBulletSFX();

            if (i < safeBulletsPerSequence - 1)
                yield return new WaitForSeconds(timeBetweenBullets);
        }

        waitedTime = adaptedWaitTimeToAttack;
        isAttacking = false;
        attackCoroutine = null;
    }

    private void ApplyDifficultySettings()
    {
        adaptedWaitTimeToAttack = waitTimeToAttack;

        if (!useAdaptiveDifficulty)
            return;

        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();

        // Si la dificultad aumenta la velocidad de enemigos, la planta espera menos entre secuencias.
        // Ejemplo:
        // enemySpeedMultiplier = 1.2 -> espera menos.
        // enemySpeedMultiplier = 0.8 -> espera más.
        adaptedWaitTimeToAttack = waitTimeToAttack / settings.enemySpeedMultiplier;

        adaptedWaitTimeToAttack = Mathf.Max(
            minimumWaitTimeToAttack,
            adaptedWaitTimeToAttack
        );
    }

    private void LaunchBullet()
    {
        if (isDead)
            return;

        if (bulletPrefab == null || launchSpawnPoint == null)
            return;

        Instantiate(
            bulletPrefab,
            launchSpawnPoint.position,
            launchSpawnPoint.rotation
        );
    }

    private void PlayBulletSFX()
    {
        if (isDead)
            return;

        if (bulletClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(bulletClip, bulletVolume);
    }

    public void DisablePlant()
    {
        isDead = true;
        isAttacking = false;

        CancelInvoke();

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        enabled = false;
    }
}