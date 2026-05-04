using UnityEngine;

public class PlantEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float waitTimeToAttack = 3f;
    [SerializeField] private float launchDelay = 0.5f;

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
        if (waitedTime <= 0f)
        {
            waitedTime = adaptedWaitTimeToAttack;

            if (animator != null)
                animator.Play("Attack");

            PlayBulletSFX();

            Invoke(nameof(LaunchBullet), launchDelay);
        }
        else
        {
            waitedTime -= Time.deltaTime;
        }
    }

    private void ApplyDifficultySettings()
    {
        adaptedWaitTimeToAttack = waitTimeToAttack;

        if (!useAdaptiveDifficulty)
            return;

        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();

        // Si la dificultad aumenta la velocidad de enemigos, la planta espera menos entre disparos.
        // Ejemplo:
        // enemySpeedMultiplier = 1.2 -> dispara algo más rápido.
        // enemySpeedMultiplier = 0.8 -> dispara más despacio.
        adaptedWaitTimeToAttack = waitTimeToAttack / settings.enemySpeedMultiplier;

        adaptedWaitTimeToAttack = Mathf.Max(
            minimumWaitTimeToAttack,
            adaptedWaitTimeToAttack
        );
    }

    private void LaunchBullet()
    {
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
        if (bulletClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(bulletClip, bulletVolume);
    }
}