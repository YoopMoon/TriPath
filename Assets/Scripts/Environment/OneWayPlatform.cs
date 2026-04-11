using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class OneWayPlatform : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private float timeToGoDown = 0.25f;

    private PlatformEffector2D platformEffector2D;
    private bool playerOnPlatform;
    private bool isDropping;

    private void Start()
    {
        platformEffector2D = GetComponent<PlatformEffector2D>();

        if (platformEffector2D == null)
        {
            Debug.LogError("BrownPlatformOn requiere un PlatformEffector2D en el mismo objeto.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!playerOnPlatform || isDropping)
            return;

        bool downPressed =
            (Keyboard.current != null &&
             ((Keyboard.current.sKey.wasPressedThisFrame) ||
              (Keyboard.current.downArrowKey.wasPressedThisFrame)));

        if (downPressed)
        {
            StartCoroutine(GoDownCoroutine());
        }
    }

    private IEnumerator GoDownCoroutine()
    {
        isDropping = true;

        platformEffector2D.rotationalOffset = 180f;

        yield return new WaitForSeconds(timeToGoDown);

        platformEffector2D.rotationalOffset = 0f;

        isDropping = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Si el jugador está encima de la plataforma,
            // la normal del contacto apunta hacia abajo.
            if (contact.normal.y < -0.5f)
            {
                playerOnPlatform = true;
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        playerOnPlatform = false;
    }
}