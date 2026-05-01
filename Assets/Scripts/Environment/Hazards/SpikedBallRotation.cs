using UnityEngine;

public class SpikedBallRotation : MonoBehaviour
{
    [Header("Rotation Mode")]
    [SerializeField] private bool fullRotation = false;

    [Header("Full Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private bool clockwise = true;

    [Header("Swing Settings")]
    [SerializeField] private float swingAngle = 180f;
    [SerializeField] private float swingGravity = 8f;
    [SerializeField] private float swingDamping = 0f;

    private Quaternion initialRotation;

    private float adaptedSpeedMultiplier = 1f;

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
        if (AdaptiveDifficultyManager.Instance == null)
            return;

        DifficultySettings settings = AdaptiveDifficultyManager.Instance.GetCurrentSettings();

        adaptedSpeedMultiplier = settings.enemySpeedMultiplier;
    }

    private void InitializeSwing()
    {
        float halfSwingAngle = swingAngle * 0.5f;

        // Empieza desde un lado del arco, como si la bola estuviera elevada.
        currentSwingAngle = clockwise ? halfSwingAngle : -halfSwingAngle;

        // Empieza sin velocidad, y la gravedad se encarga de acelerar la caída.
        swingAngularVelocity = 0f;
    }

    private void RotateFullCircle()
    {
        float direction = clockwise ? -1f : 1f;

        transform.Rotate(
            0f,
            0f,
            rotationSpeed * adaptedSpeedMultiplier * direction * Time.deltaTime
        );
    }

    private void SwingWithGravity()
    {
        float halfSwingAngle = swingAngle * 0.5f;

        // Usamos un delta adaptado por dificultad.
        // En Hard el péndulo evoluciona más rápido; en Easy, más lento.
        float deltaTime = Time.deltaTime * adaptedSpeedMultiplier;

        // Simulación básica de péndulo:
        // cuanto más inclinado está, más acelera hacia el centro.
        float angleInRadians = currentSwingAngle * Mathf.Deg2Rad;
        float angularAcceleration = -swingGravity * Mathf.Sin(angleInRadians);

        swingAngularVelocity += angularAcceleration * deltaTime;

        // Damping opcional. Si lo dejas a 0, mantiene el movimiento constante.
        if (swingDamping > 0f)
            swingAngularVelocity *= 1f - swingDamping * deltaTime;

        currentSwingAngle += swingAngularVelocity * Mathf.Rad2Deg * deltaTime;

        // Seguridad para que no se pase del arco definido.
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