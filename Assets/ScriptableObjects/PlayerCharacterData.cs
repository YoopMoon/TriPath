using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCharacterData", menuName = "Game/Player Character Data")]
public class PlayerCharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;

    [Header("Visuals")]
    public Sprite idleSprite;
    public RuntimeAnimatorController animatorController;

    [Header("Gameplay")]
    public float moveSpeed = 1.5f;
    public bool hasDoubleJump = false;
    public int maxHearts = 3;
}