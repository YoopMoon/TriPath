using UnityEngine;
using UnityEngine.UI;

public class DifficultyUI : MonoBehaviour
{
    [SerializeField] private Text difficultyText;
    [SerializeField] public bool difficultyDisplayed = true;

    private void Start()
    {
        if (difficultyText == null)
            difficultyText = GetComponent<Text>();

        if (AdaptiveDifficultyManager.Instance != null && difficultyDisplayed)
        {
            AdaptiveDifficultyManager.Instance.OnDifficultyChanged += UpdateDifficultyText;
            UpdateDifficultyText(AdaptiveDifficultyManager.Instance.CurrentDifficulty);
        }
    }

    private void OnDestroy()
    {
        if (AdaptiveDifficultyManager.Instance != null)
            AdaptiveDifficultyManager.Instance.OnDifficultyChanged -= UpdateDifficultyText;
    }

    private void UpdateDifficultyText(AdaptiveDifficulty difficulty)
    {
        if (difficultyDisplayed) {
            difficultyText.text = "Mode: " + GetDifficultyName(difficulty);

            switch (difficulty)
            {
                case AdaptiveDifficulty.Easy:
                    difficultyText.color = new Color(0.2f, 0.9f, 0.2f);
                    break;

                case AdaptiveDifficulty.Hard:
                    difficultyText.color = new Color(0.9f, 0.2f, 0.2f);
                    break;

                case AdaptiveDifficulty.Normal:
                default:
                    difficultyText.color = Color.white;
                    break;
            }
        }
    }

    private string GetDifficultyName(AdaptiveDifficulty difficulty)
    {
        switch (difficulty)
        {
            case AdaptiveDifficulty.Easy:
                return "Easy";

            case AdaptiveDifficulty.Hard:
                return "Hard";

            case AdaptiveDifficulty.Normal:
            default:
                return "Normal";
        }
    }
}