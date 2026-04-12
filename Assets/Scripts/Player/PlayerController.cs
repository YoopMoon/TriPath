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
    public float jumpForce = 2.8f;
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

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private InputAction jumpAction;

    public SpriteRenderer spriteRenderer;
    public Animator animator;

    public bool isInmortal = false;

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

    private void HandleJumpInput()
    {
        // El salto se detecta en Update y no en FixedUpdate porque Update se ejecuta
        // una vez por frame y captura mejor pulsaciones cortas del botón.
        // Si leyéramos el input solo en FixedUpdate, podría ocurrir que el jugador
        // pulsase y soltase el botón entre dos pasos de física y ese salto se perdiera.
        if (!jumpAction.WasPressedThisFrame())
            return;

        // Si está en el suelo, permito el salto normal.
        // Si está en el aire pero todavía conserva el segundo salto,
        // también registro el salto para consumirlo después.
        if (FloorChecker.isFloorDetected || (doubleJumpEnabled && canDoubleJump))
            jumpPressed = true;
    }

    private void HandleMovement()
    {
        bool isFloorDetected = FloorChecker.isFloorDetected;

        // Cada vez que el personaje toca el suelo, recupera la posibilidad
        // de hacer un doble salto en el siguiente salto aéreo.
        if (isFloorDetected)
        {
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
        // El salto sí se aplica en FixedUpdate porque aquí estamos modificando
        // directamente la velocidad del Rigidbody2D, es decir, física del personaje.
        if (!jumpPressed)
            return;

        if (FloorChecker.isFloorDetected)
        {
            // Salto normal desde el suelo.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // El primer salto no debe activar la animación de doble salto.
            animator.SetBool("DoubleJump", false);
        }
        else if (doubleJumpEnabled && canDoubleJump)
        {
            // Segundo salto en el aire.
            // Se desactiva después de usarlo para que solo pueda hacerse una vez.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
            canDoubleJump = false;

            // Activo la animación específica del doble salto
            // solo cuando realmente se consume el segundo salto.
            animator.SetBool("DoubleJump", true);
        }

        // Consumo la pulsación para evitar saltos pendientes.
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

        if (!doubleJumpEnabled)
            canDoubleJump = false;
    }
}