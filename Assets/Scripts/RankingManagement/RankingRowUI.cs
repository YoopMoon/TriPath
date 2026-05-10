using UnityEngine;
using UnityEngine.UI;

public class RankingRowUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private Text rankText;
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text characterText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text coinsText;
    [SerializeField] private Text damageText;
    [SerializeField] private Text timeText;

    public void Setup(int rankPosition, RankingEntry entry)
    {
        if (entry == null)
            return;

        if (rankText != null)
            rankText.text = $"#{rankPosition}";

        if (playerNameText != null)
            playerNameText.text = FormatPlayerName(entry.playerName).ToUpper();

        if (characterText != null)
            characterText.text = FormatCharacterName(entry.characterName);

        if (scoreText != null)
            scoreText.text = entry.finalScore.ToString();

        if (coinsText != null)
            coinsText.text = $"{entry.totalCoinsCollected}/{entry.totalCoinsAvailable}";

        if (damageText != null)
            damageText.text = entry.totalDamageTaken.ToString();

        if (timeText != null)
            timeText.text = FormatTime(entry.totalTime);
    }

    private string FormatPlayerName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            return "PLAYER";

        playerName = playerName.Trim();

        if (playerName.Length > 10)
            playerName = playerName.Substring(0, 10);

        return playerName;
    }

    private string FormatCharacterName(string characterName)
    {
        switch (characterName)
        {
            case "VirtualGuy":
                return "Virtual Guy";
            case "MaskDude":
                return "Mask Dude";
            case "NinjaFrog":
                return "Ninja Frog";
            case "Frog":
                return "Frog";
            default:
                return characterName;
        }
    }

    private string FormatTime(float totalTime)
    {
        int seconds = Mathf.RoundToInt(totalTime);
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        return $"{minutes:00}:{remainingSeconds:00}";
    }
}