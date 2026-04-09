using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float jumpForce = 4f;

    [Header("Jump Feel")]
    public bool enhancedJump = true;
    public float fallGravityMultiplier = 0.5f; // cuánto más rápido cae al bajar
    public float jumpCutMultiplier = 2f;        // cuánto se corta el salto al soltar el botón
    private bool jumpPressed;

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

        // busco la acción Jump directamente desde el PlayerInput del objeto
        jumpAction = GetComponent<PlayerInput>().actions["Jump"];

        playerHealth = GetComponent<PlayerHealth>();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        // solo registro el salto si está en el suelo en el momento de pulsar
        if (value.isPressed && GroundChecker.isGrounded)
            jumpPressed = true;
    }

    void FixedUpdate()
    {
        // movimiento horizontal, mantengo la velocidad vertical que ya lleva
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // actualizo la dirección del sprite solo si hay movimiento, para mantener la última posición al soltar
        if (moveInput.x != 0)
            spriteRenderer.flipX = moveInput.x < 0;

        // activo la animación de correr solo si hay input horizontal y está en el suelo
        animator.SetBool("Run", moveInput.x != 0 && GroundChecker.isGrounded);

        // salto: consumo jumpPressed siempre para evitar saltos pendientes al aterrizar
        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
        }

        // animaciones de salto y caída según velocidad vertical y si está en el suelo
        bool isFalling = rb.linearVelocity.y < 0f && !GroundChecker.isGrounded;
        animator.SetBool("Jump", !GroundChecker.isGrounded && !isFalling);
        animator.SetBool("Fall", isFalling);

        if (playerHealth.hitReceived && !isPlayingHitAnimation)
        {
            StartCoroutine(PlayHitAnimation());
        }

        if (enhancedJump)
        {
            // leo el estado del botón aquí directamente para evitar desfases con el callback
            bool jumpHeld = jumpAction.IsPressed();

            if (rb.linearVelocity.y < 0f)
            {
                // cayendo: aplico gravedad extra para que la caída se sienta más pesada
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * fallGravityMultiplier * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0f && !jumpHeld)
            {
                // subiendo pero soltó el botón: corto el salto con gravedad extra
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * jumpCutMultiplier * Time.fixedDeltaTime;
            }
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
}