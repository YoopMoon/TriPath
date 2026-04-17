using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystemUI : MonoBehaviour
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

    [Header("Damage Blink")]
    [SerializeField] private float damageBlinkDuration = 0.35f;
    [SerializeField] private float damageBlinkInterval = 0.08f;

    [Header("Heal Blink")]
    [SerializeField] private float healBlinkDuration = 0.5f;
    [SerializeField] private float healBlinkInterval = 0.14f;

    private Coroutine blinkCoroutine;

    private void Start()
    {
        playerHealth.OnHealthChanged += UpdateHearts;
        playerHealth.OnDamageTaken += BlinkHeartsOnDamage;
        playerHealth.OnHealed += BlinkHeartsOnHeal;

        UpdateHearts();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHearts;
            playerHealth.OnDamageTaken -= BlinkHeartsOnDamage;
            playerHealth.OnHealed -= BlinkHeartsOnHeal;
        }
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

            heartImages[i].enabled = true;
        }
    }

    private void BlinkHeartsOnDamage()
    {
        StartBlink(damageBlinkDuration, damageBlinkInterval);
    }

    private void BlinkHeartsOnHeal()
    {
        StartBlink(healBlinkDuration, healBlinkInterval);
    }

    private void StartBlink(float duration, float interval)
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkHeartsCoroutine(duration, interval));
    }

    private IEnumerator BlinkHeartsCoroutine(float duration, float interval)
    {
        float elapsedTime = 0f;
        bool visible = true;

        while (elapsedTime < duration)
        {
            visible = !visible;

            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i].gameObject.activeSelf)
                    heartImages[i].enabled = visible;
            }

            yield return new WaitForSeconds(interval);
            elapsedTime += interval;
        }

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i].gameObject.activeSelf)
                heartImages[i].enabled = true;
        }

        blinkCoroutine = null;
    }
}