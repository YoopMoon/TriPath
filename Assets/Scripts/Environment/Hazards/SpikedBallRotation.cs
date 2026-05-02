using UnityEngine;

public class SpikedBallRotation : MonoBehaviour
{
    [Header("Rotation Mode")]
    [SerializeField] private bool fullRotation = false;

    [Header("Full Rotation Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool clockwise = true;

    [Header("Swing Settings")]
    [SerializeField] private float swingAngle = 180f;
    [SerializeField] private float swingGravity = 4f;
    [SerializeField] private float swingDamping = 0f;

    [Header("Adaptive Difficulty")]
    [SerializeField] private bool useAdaptiveDifficulty = true;

    // Influencia completa para la bola que gira 360 grados.
    // En este caso sí interesa que el multiplicador de dificultad afecte más directamente.
    [SerializeField] private float fullRotationAdaptiveInfluence = 1f;

    // Influencia reducida para la bola pendular.
    // El péndulo ya acelera de forma natural por la gravedad, por eso conviene
    // que la dificultad no multiplique todo el movimiento de forma tan agresiva.
    [SerializeField] private float swingAdaptiveInfluence = 0.35f;

    // Límites para evitar que el péndulo quede demasiado lento en Easy
    // o demasiado rápido en Hard.
    [SerializeField] private float minSwingMultiplier = 0.75f;
    [SerializeField] private float maxSwingMultiplier = 1.25f;

    private Quaternion initialRotation;

    private float fullRotationMultiplier = 1f;
    private float swingMultiplier = 1f;

    private float currentSwingAngle;
    private float swingAngularVelocity;

    private void Awake()
    {
        initialRotation = transform.rotation;

        ApplyDifficultySettings();
        InitializeSwing();
    }

    private void Update()
    {
        if (fullRotation)
            RotateFullCircle();
        else
            SwingWithGravity();
    }

    private void ApplyDifficultySettings()
    {
        // Valores base si no hay sistema adaptativo o está desactivado.
        fullRotationMultiplier = 1f;
        swingMultiplier = 1f;

        if (!useAdaptiveDifficulty)
            return;

        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();

        float enemySpeedMultiplier = settings.enemySpeedMultiplier;

        // Para rotación completa, interpolamos hacia el multiplicador adaptativo.
        // Con influencia 1, aplica el multiplicador completo.
        // Con influencia 0, ignora la dificultad.
        fullRotationMultiplier = Mathf.Lerp(
            1f,
            enemySpeedMultiplier,
            fullRotationAdaptiveInfluence
        );

        // Para el péndulo usamos una influencia menor para evitar movimientos exagerados.
        // No se acelera el tiempo de simulación, solo se adapta la gravedad efectiva.
        swingMultiplier = Mathf.Lerp(
            1f,
            enemySpeedMultiplier,
            swingAdaptiveInfluence
        );

        swingMultiplier = Mathf.Clamp(
            swingMultiplier,
            minSwingMultiplier,
            maxSwingMultiplier
        );
    }

    private void InitializeSwing()
    {
        float halfSwingAngle = swingAngle * 0.5f;

        // Empieza desde un lado del arco, como si la bola estuviera elevada.
        currentSwingAngle = clockwise ? halfSwingAngle : -halfSwingAngle;

        // Empieza sin velocidad inicial y la gravedad se encarga de acelerar la caída.
        swingAngularVelocity = 0f;
    }

    private void RotateFullCircle()
    {
        float direction = clockwise ? -1f : 1f;

        transform.Rotate(
            0f,
            0f,
            rotationSpeed * fullRotationMultiplier * direction * Time.deltaTime
        );
    }

    private void SwingWithGravity()
    {
        float halfSwingAngle = swingAngle * 0.5f;

        // En el péndulo mantenemos Time.deltaTime normal.
        // La dificultad se aplica sobre la gravedad, no sobre el tiempo completo.
        float deltaTime = Time.deltaTime;

        float adaptedGravity = swingGravity * swingMultiplier;

        // Simulación básica de péndulo:
        // cuanto más inclinado está, más acelera hacia el centro.
        float angleInRadians = currentSwingAngle * Mathf.Deg2Rad;
        float angularAcceleration = -adaptedGravity * Mathf.Sin(angleInRadians);

        swingAngularVelocity += angularAcceleration * deltaTime;

        // Damping opcional. Si se deja a 0, el péndulo mantiene el movimiento.
        // Si se aumenta, irá perdiendo fuerza poco a poco.
        if (swingDamping > 0f)
        {
            float dampingFactor = Mathf.Clamp01(1f - swingDamping * deltaTime);
            swingAngularVelocity *= dampingFactor;
        }

        currentSwingAngle += swingAngularVelocity * Mathf.Rad2Deg * deltaTime;

        // Seguridad para que no se salga del arco definido.
        if (currentSwingAngle > halfSwingAngle)
        {
            currentSwingAngle = halfSwingAngle;
            swingAngularVelocity = 0f;
        }
        else if (currentSwingAngle < -halfSwingAngle)
        {
            currentSwingAngle = -halfSwingAngle;
            swingAngularVelocity = 0f;
        }

        transform.rotation = initialRotation * Quaternion.Euler(0f, 0f, currentSwingAngle);
    }
}