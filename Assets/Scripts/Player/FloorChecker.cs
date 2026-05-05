using UnityEngine;

// Este script va en el objeto hijo situado debajo del personaje.
// Ese objeto tiene un BoxCollider2D en modo Trigger que detecta el suelo.
//
// No basta con poner true al entrar y false al salir, porque el checker puede
// estar tocando varios colliders de suelo a la vez. En ese caso, salir de uno
// no significa necesariamente haber dejado de tocar el suelo por completo.
//
// Por eso hay un contador de contactos con suelo y solo se considera que
// el personaje ha dejado el suelo cuando ese contador llega a 0.
public class FloorChecker : MonoBehaviour
{
    // Lo dejo estático para seguir accediendo desde PlayerController
    // sin necesidad de referencias adicionales.
    public static bool isFloorDetected;

    // Cuenta cuántos colliders de suelo está tocando ahora mismo el checker.
    private int floorContacts = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Floor") && !collision.CompareTag("FloorOneWayPlatform"))
            return;

        floorContacts++;

        isFloorDetected = floorContacts > 0;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Floor") && !collision.CompareTag("FloorOneWayPlatform"))
            return;

        floorContacts--;

        // Evito que, por seguridad, el contador pueda quedarse en negativo
        // si Unity lanza algún evento inesperado o se destruye un collider.
        if (floorContacts < 0)
            floorContacts = 0;

        isFloorDetected = floorContacts > 0;
    }

    private void OnDisable()
    {
        // Si el objeto se desactiva, reiniciamos el estado para no arrastrar
        // una detección de suelo incorrecta.
        floorContacts = 0;
        isFloorDetected = false;
    }
}