using UnityEngine;

public class PlayerSelect : MonoBehaviour
{
    public enum Player
    {
        Frog,
        VirtualGuy,
        MaskDude
    }

    [Header("Selected Player")]
    public Player selectedPlayer;

    [Header("Character Data")]
    public PlayerCharacterData frogData;
    public PlayerCharacterData virtualGuyData;
    public PlayerCharacterData maskDudeData;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerController playerController;
    private PlayerHealth playerHealth;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();

        selectedPlayer = SelectedPlayerStore.SelectedPlayer;

        ApplySelectedPlayer();
    }

    private void ApplySelectedPlayer()
    {
        PlayerCharacterData selectedData = GetSelectedCharacterData();

        if (selectedData == null)
        {
            Debug.LogWarning("No hay datos asignados para el personaje seleccionado.");
            return;
        }

        if (spriteRenderer != null && selectedData.idleSprite != null)
            spriteRenderer.sprite = selectedData.idleSprite;

        if (animator != null && selectedData.animatorController != null)
            animator.runtimeAnimatorController = selectedData.animatorController;

        if (playerController != null)
        {
            playerController.moveSpeed = selectedData.moveSpeed;
            playerController.SetDoubleJumpEnabled(selectedData.hasDoubleJump);
        }

        if (playerHealth != null)
        {
            playerHealth.SetMaxHearts(selectedData.maxHearts);
        }
    }

    private PlayerCharacterData GetSelectedCharacterData()
    {
        switch (selectedPlayer)
        {
            case Player.Frog:
                return frogData;

            case Player.VirtualGuy:
                return virtualGuyData;

            case Player.MaskDude:
                return maskDudeData;

            default:
                return null;
        }
    }
}