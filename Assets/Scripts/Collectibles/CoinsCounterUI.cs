using UnityEngine;
using UnityEngine.UI;

public class CoinsCounterUI : MonoBehaviour
{
    [SerializeField] private Text coinText;

    private void Start()
    {
        if (coinText == null)
            coinText = GetComponent<Text>();

        if (SceneTransitionManager.instance != null)
        {
            SceneTransitionManager.instance.OnCoinsChanged += UpdateCoinText;
            UpdateCoinText(SceneTransitionManager.instance.GetPlayerCoins());
        }
    }

    private void OnDestroy()
    {
        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.OnCoinsChanged -= UpdateCoinText;
    }

    private void UpdateCoinText(int coins)
    {
        coinText.text = coins.ToString();
    }
}