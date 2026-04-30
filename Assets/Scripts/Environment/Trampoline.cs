using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Bounce Settings")]
    [SerializeField] private float jumpForce = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip bounceClip;
    [SerializeField] private float bounceVolume = 1f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Player"))
            return;

        if (!IsPlayerComingFromAbove(collision))
            return;

        if (!collision.gameObject.TryGetComponent(out Rigidbody2D playerRb))
            return;

        playerRb.linearVelocity = Vector2.up * jumpForce;

        if (animator != null)
            animator.Play("TrampolineJump", 0, 0f);

        PlayBounceSFX();
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