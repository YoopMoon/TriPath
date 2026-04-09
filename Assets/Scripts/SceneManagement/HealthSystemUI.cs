using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image[] heartImages;

    [Header("Heart Sprites")]
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Sprite quarterHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite threeQuarterHeart;
    [SerializeField] private Sprite fullHeart;

    private void Start()
    {
        playerHealth.OnHealthChanged += UpdateHearts;
        UpdateHearts();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHearts;
    }

    private void UpdateHearts()
    {
        int currentHealth = playerHealth.CurrentHealth;
        int maxHealth = playerHealth.MaxHealth;

        for (int i = 0; i < heartImages.Length; i++)
        {
            int heartStartValue = i * 4;

            if (heartStartValue >= maxHealth)
            {
                heartImages[i].gameObject.SetActive(false);
                continue;
            }

            heartImages[i].gameObject.SetActive(true);

            int fillAmount = Mathf.Clamp(currentHealth - heartStartValue, 0, 4);

            switch (fillAmount)
            {
                case 0:
                    heartImages[i].sprite = emptyHeart;
                    break;
                case 1:
                    heartImages[i].sprite = quarterHeart;
                    break;
                case 2:
                    heartImages[i].sprite = halfHeart;
                    break;
                case 3:
                    heartImages[i].sprite = threeQuarterHeart;
                    break;
                case 4:
                    heartImages[i].sprite = fullHeart;
                    break;
            }
        }
    }
}