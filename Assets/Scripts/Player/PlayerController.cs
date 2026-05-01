using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.8f;
    public float floorAcceleration = 16f;
    public float floorDeceleration = 22f;
    public float airAcceleration = 6f;
    public float airDeceleration = 8f;
    public float turnAccelerationMultiplier = 1.3f;
    public float jumpForce = 3f;
    public float doubleJumpForce = 2.5f;
    private bool canDoubleJump;
    private bool doubleJumpEnabled = true;

    [Header("Jump Feel")]
    public bool enhancedJump = true;
    public float fallGravityMultiplier = 0.5f; // cuánto más rápido cae al bajar
    public float jumpCutMultiplier = 1f;       // cuánto se corta el salto al soltar el botón
    private bool jumpPressed;

    [Header("Animation Thresholds")]
    public float runAnimationThreshold = 0.05f;
    public float verticalAnimationThreshold = 0.05f;

    [Header("Hit Animation")]
    public float hitAnimationDuration = 0.1f;
    private bool isPlayingHitAnimation = false;

    [Header("Jump Grace Time")]
    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private InputAction jumpAction;

    [Header("Player SpriteRender and Animator")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private float jumpVolume = 1f;

    [SerializeField] private AudioClip[] stepClips;
    [SerializeField] private float stepVolume = 1f;

    private int currentStepClipIndex = 0;
    public PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Busco la acción Jump directamente desde el PlayerInput del objeto.
        jumpAction = GetComponent<PlayerInput>().actions["Jump"];

        playerHealth = GetComponent<PlayerHealth>();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        UpdateCoyoteTime();
        HandleJumpInput();
        HandleAnimations();
        HandleHitAnimation();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        HandleEnhancedJumpPhysics();
    }

    private void UpdateCoyoteTime()
    {
        if (FloorChecker.isFloorDetected)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;
    }

    private void HandleJumpInput()
    {
        // El salto se detecta en Update y no en FixedUpdate porque Update se ejecuta
        // una vez por frame y captura mejor pulsaciones cortas del botón.
        // Si leyéramos el input solo en FixedUpdate, podría ocurrir que el jugador
        // pulsase y soltase el botón entre dos pasos de física y ese salto se perdiera.
        if (!jumpAction.WasPressedThisFrame())
            return;

        // Permitimos registrar el salto si todavía estamos dentro del margen del
        // coyote time o si el personaje conserva el doble salto.
        bool canUseNormalJump = coyoteTimeCounter > 0f;
        bool canUseDoubleJump = doubleJumpEnabled && canDoubleJump;

        if (canUseNormalJump || canUseDoubleJump)
            jumpPressed = true;
    }

    private void HandleMovement()
    {
        bool isFloorDetected = FloorChecker.isFloorDetected;

        // Cada vez que el personaje toca el suelo, recupera la posibilidad
        // de hacer un doble salto en el siguiente salto aéreo.
        if (isFloorDetected)
        {
            if (doubleJumpEnabled)
                canDoubleJump = true;

            // Al volver al suelo se desactiva la animación de doble salto,
            // ya que el siguiente salto volverá a ser el primero.
            animator.SetBool("DoubleJump", false);
        }

        float targetSpeed = moveInput.x * moveSpeed;
        float currentSpeed = rb.linearVelocity.x;
        float speedDifference = targetSpeed - currentSpeed;

        // En suelo el personaje responde más rápido y se frena mejor.
        // En aire mantengo algo de control, pero más suave para que no se sienta artificial.
        float accelerationRate = 0f;

        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            accelerationRate = isFloorDetected ? floorAcceleration : airAcceleration;

            // Si está cambiando de dirección, acelero un poco más
            // para que el giro no se sienta torpe.
            if (Mathf.Abs(currentSpeed) > 0.01f && Mathf.Sign(targetSpeed) != Mathf.Sign(currentSpeed))
                accelerationRate *= turnAccelerationMultiplier;
        }
        else
        {
            accelerationRate = isFloorDetected ? floorDeceleration : airDeceleration;
        }

        // MoveTowards da una aceleración/desaceleración más controlada y estable
        // que sumar directamente una fracción del speedDifference.
        float newXVelocity = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            accelerationRate * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);

        // Actualizo la dirección del sprite solo si realmente hay movimiento horizontal,
        // para mantener la última orientación al detenerse.
        if (Mathf.Abs(rb.linearVelocity.x) > 0.01f)
            spriteRenderer.flipX = rb.linearVelocity.x < 0f;
    }

    private void HandleJump()
    {
        // El salto se aplica en FixedUpdate porque aquí modificamos directamente
        // la velocidad del Rigidbody2D, es decir, la física del personaje.
        if (!jumpPressed)
            return;

        // El salto normal no depende solo de que el suelo se detecte justo en este frame.
        // Gracias al coyote time, permitimos saltar durante un pequeño margen de tiempo
        // después de haber dejado de tocar el suelo. Esto evita fallos de salto cuando
        // la detección del suelo parpadea brevemente o cuando el personaje cambia muy rápido
        // de dirección sobre una plataforma.
        bool canUseNormalJump = coyoteTimeCounter > 0f;

        if (canUseNormalJump)
        {
            // Salto normal desde el suelo o dentro del margen del coyote time.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // Reproducimos el sonido una sola vez, justo cuando el salto ocurre de verdad.
            PlayJumpSFX();

            // Consumimos el coyote time para que no pueda reutilizarse varias veces
            // con una sola detección de suelo.
            coyoteTimeCounter = 0f;

            // El salto normal no debe activar la animación de doble salto.
            animator.SetBool("DoubleJump", false);
        }
        else if (doubleJumpEnabled && canDoubleJump)
        {
            // Segundo salto en el aire.
            // Solo puede ejecutarse si este personaje tiene doble salto habilitado
            // y todavía no lo ha consumido durante este salto.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
            canDoubleJump = false;

            // Si quieres que el doble salto use también el mismo sonido, déjalo aquí.
            PlayJumpSFX();

            // Activo la animación específica del doble salto
            // solo cuando realmente se consume el segundo salto.
            animator.SetBool("DoubleJump", true);
        }

        // Consumimos la pulsación para evitar saltos pendientes.
        jumpPressed = false;
    }

    private void HandleAnimations()
    {
        bool isFloorDetected = FloorChecker.isFloorDetected;
        bool isRising = rb.linearVelocity.y > verticalAnimationThreshold && !isFloorDetected;
        bool isFalling = rb.linearVelocity.y < -verticalAnimationThreshold && !isFloorDetected;

        // La animación de correr se basa en la velocidad real y no solo en el input,
        // para que acompañe mejor la aceleración progresiva del personaje.
        animator.SetBool("Run", Mathf.Abs(rb.linearVelocity.x) > runAnimationThreshold && isFloorDetected);

        // Animaciones de salto y caída según velocidad vertical y si está en el suelo.
        animator.SetBool("Jump", isRising);
        animator.SetBool("Fall", isFalling);

        // El doble salto solo debe mantenerse durante el impulso del segundo salto.
        // En cuanto empieza la caída o el personaje toca el suelo, se desactiva.
        if (isFalling || isFloorDetected)
            animator.SetBool("DoubleJump", false);
    }

    private void HandleHitAnimation()
    {
        if (playerHealth.hitReceived && !isPlayingHitAnimation)
        {
            StartCoroutine(PlayHitAnimation());
        }
    }

    private void HandleEnhancedJumpPhysics()
    {
        if (!enhancedJump)
            return;

        // Leo el estado del botón aquí para ajustar la física del salto.
        bool jumpHeld = jumpAction.IsPressed();

        if (rb.linearVelocity.y < 0f)
        {
            // Cayendo: aplico gravedad extra para que la caída se sienta más pesada.
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * fallGravityMultiplier * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            // Subiendo pero soltó el botón: corto el salto con gravedad extra.
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * jumpCutMultiplier * Time.fixedDeltaTime;
        }
    }

    private IEnumerator PlayHitAnimation()
    {
        isPlayingHitAnimation = true;
        playerHealth.hitReceived = false;

        animator.SetBool("Hit", true);
        yield return new WaitForSeconds(hitAnimationDuration);
        animator.SetBool("Hit", false);

        isPlayingHitAnimation = false;
    }

    public void SetDoubleJumpEnabled(bool enabled)
    {
        doubleJumpEnabled = enabled;
        canDoubleJump = enabled && FloorChecker.isFloorDetected;
    }

    private void PlayJumpSFX()
    {
        if (jumpClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(jumpClip, jumpVolume);
    }

    public void PlayStepSFX()
    {
        // Si no hay clips configurados, no hacemos nada.
        if (stepClips == null || stepClips.Length == 0)
            return;

        // Evita que suene si el personaje no está realmente en el suelo.
        if (!FloorChecker.isFloorDetected)
            return;

        // Evita que suene si prácticamente no se está moviendo.
        if (Mathf.Abs(rb.linearVelocity.x) < runAnimationThreshold)
            return;

        AudioClip clipToPlay = stepClips[currentStepClipIndex];

        // Avanzamos al siguiente clip para alternar los dos pasos.
        currentStepClipIndex++;
        if (currentStepClipIndex >= stepClips.Length)
            currentStepClipIndex = 0;

        if (clipToPlay == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(clipToPlay, stepVolume);
    }
}