using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerPlayer : MonoBehaviour
{
    // velocidad de movimiento horizontal y fuerza del salto
    // los dejo públicos para poder tocarlos desde el inspector sin recompilar
    public float runSpeed = 2f;
    public float jumpForce = 3f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpPressed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // estos métodos los llama automáticamente el Player Input
    // según las acciones que definí en el Input Actions Asset

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        // guardo si el botón está pulsado para procesarlo en FixedUpdate
        // no lo gestiono aquí directamente porque las físicas van en FixedUpdate
        jumpPressed = value.isPressed;
    }

    void FixedUpdate()
    {
        // muevo el personaje en X manteniendo la velocidad en Y que ya lleva
        // si no hago esto, la gravedad y el salto dejan de funcionar bien
        rb.linearVelocity = new Vector2(moveInput.x * runSpeed, rb.linearVelocity.y);

        // solo salto si el jugador ha pulsado el botón Y está tocando el suelo
        // la comprobación de suelo la hace un script auxiliar con un trigger en los pies
        if (jumpPressed && GroundChecker.isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false; // reseteo para que no salte otra vez en el siguiente frame
        }
    }
}