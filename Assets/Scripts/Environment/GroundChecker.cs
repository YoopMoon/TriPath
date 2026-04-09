using UnityEngine;

// este script va en el objeto hijo que puse debajo del personaje
// ese objeto tiene un BoxCollider2D en modo Trigger que detecta el suelo
// cuando ese collider toca algo, actualizamos el booleano que usa ControllerPlayer

public class GroundChecker : MonoBehaviour
{
    // lo dejo estático para acceder a él desde ControllerPlayer sin necesidad
    // de guardar una referencia al objeto, más sencillo por ahora
    public static bool isGrounded;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        isGrounded = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isGrounded = false;
    }
}