using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 originalPosition;
    private bool isResetting = false;

    [SerializeField] private float delayToGoDown = 1f;
    [SerializeField] private float delayToReset = 3f;
    [SerializeField] public float returnSpeed = 3f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TriggerFall(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TriggerFall(collision);
    }

    private void TriggerFall(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isResetting)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    StartCoroutine(Fall());
                    break;
                }
            }
        }
    }

    private IEnumerator Fall()
    {
        isResetting = true;

        // Espera y cae (quita solo el freeze de Y)
        yield return new WaitForSeconds(delayToGoDown);
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        // Espera y vuelve despacio
        yield return new WaitForSeconds(delayToReset);
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        while (Vector3.Distance(transform.position, originalPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = originalPosition;
        isResetting = false;
    }
}