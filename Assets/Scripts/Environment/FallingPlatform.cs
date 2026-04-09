using UnityEngine;
using System.Collections;

public class BouncePlatform : MonoBehaviour
{
    private Vector3 originalPosition;
    private bool isAnimating = false;

    public bool isEnabled = true;

    private readonly float dropAmount = 0.025f;     // cuánto baja
    private readonly float dropSpeed = 2f;         // velocidad de bajada
    private readonly float bounceSpeed = 1f;       // velocidad de subida/rebote
    private readonly float holdAtBottom = 0.1f;    // pausa abajo antes de subir

    void Start()
    {
        originalPosition = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isAnimating && isEnabled)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    StartCoroutine(BounceSequence());
                    break;
                }
            }
        }
    }

    private IEnumerator BounceSequence()
    {
        isAnimating = true;

        Vector3 downTarget = originalPosition + Vector3.down * dropAmount;

        // Baja hasta el punto de destino
        while (Vector3.Distance(transform.position, downTarget) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, downTarget, dropSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = downTarget;

        // Pausa breve abajo
        yield return new WaitForSeconds(holdAtBottom);

        // Sube de vuelta al origen
        while (Vector3.Distance(transform.position, originalPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, bounceSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = originalPosition;

        isAnimating = false;
    }
}