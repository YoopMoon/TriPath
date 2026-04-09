using UnityEngine;

// este script va en el objeto hijo que puse debajo del personaje
// ese objeto tiene un BoxCollider2D en modo Trigger que detecta el suelo
// cuando ese collider toca algo, actualizamos el booleano que usa ControllerPlayer

public class FloorChecker : MonoBehaviour
{
    // lo dejo estático para acceder a él desde ControllerPlayer sin necesidad
    // de guardar una referencia al objeto, más sencillo por ahora
    public static bool isFloorDetected;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Floor"))
        {
            isFloorDetected = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Floor"))
        {
            isFloorDetected = false;
        }
    }
}