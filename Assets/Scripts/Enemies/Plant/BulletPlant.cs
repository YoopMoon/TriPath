using UnityEngine;

public class BulletPlant : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float lifeTime = 2f;

    [Header("Direction")]
    [SerializeField] private bool left; // Indica hacia dónde va la bala.

    private float adaptedSpeed;
    private float adaptedLifeTime;

    private void Awake()
    {
        ApplyDifficultySettings();
    }

    private void Start()
    {
        Destroy(gameObject, adaptedLifeTime);
    }

    private void Update()
    {
        Vector2 direction = left ? Vector2.left : Vector2.right;
        transform.Translate(direction * adaptedSpeed * Time.deltaTime);
    }

    private void ApplyDifficultySettings()
    {
        adaptedSpeed = speed;
        adaptedLifeTime = lifeTime;

        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();

        // Reutilizamos el multiplicador de velocidad de enemigos para que las balas
        // también sean más o menos exigentes según la dificultad actual.
        adaptedSpeed = speed * settings.enemySpeedMultiplier;

        adaptedSpeed = Mathf.Max(0.1f, adaptedSpeed);
        adaptedLifeTime = Mathf.Max(0.1f, adaptedLifeTime);
    }

    public void SetDirection(bool moveLeft)
    {
        left = moveLeft;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0.1f, newSpeed);
        ApplyDifficultySettings();
    }
}