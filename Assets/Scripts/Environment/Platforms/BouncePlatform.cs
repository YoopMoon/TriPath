using UnityEngine;
using System.Collections;

public class BouncePlatform : MonoBehaviour
{
    private Vector2 originalPosition;
    private bool isAnimating = false;
    private Rigidbody2D rb;

    public bool isEnabled = true;

    private readonly float dropAmount = 0.025f;   // cuánto baja
    private readonly float dropSpeed = 2f;        // velocidad de bajada
    private readonly float bounceSpeed = 1f;      // velocidad de subida/rebote
    private readonly float holdAtBottom = 0.1f;   // pausa abajo antes de subir

    [Header("Audio")]
    [SerializeField] private AudioClip bounceClip;
    [SerializeField] private float bounceVolume = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalPosition = rb.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isEnabled)
        {
            if (!IsPlayerComingFromAbove(collision))
                return;

            PlayBounceSFX();
        }

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

        Vector2 downTarget = originalPosition + Vector2.down * dropAmount;

        // Baja hasta el punto de destino
        while (Vector2.Distance(rb.position, downTarget) > 0.001f)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, downTarget, dropSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(downTarget);

        // Sonido justo al terminar la bajada
        PlayBounceSFX();

        // Pausa breve abajo
        yield return new WaitForSeconds(holdAtBottom);

        // Sube de vuelta al origen
        while (Vector2.Distance(rb.position, originalPosition) > 0.001f)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, originalPosition, bounceSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(originalPosition);

        isAnimating = false;
    }

    private bool IsPlayerComingFromAbove(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Si la normal apunta hacia abajo desde el punto de vista del jugador,
            // significa que el jugador ha tocado la parte superior del trampolín.
            if (contact.normal.y < -0.5f)
                return true;
        }

        return false;
    }

    private void PlayBounceSFX()
    {
        if (bounceClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(bounceClip, bounceVolume);
    }
}