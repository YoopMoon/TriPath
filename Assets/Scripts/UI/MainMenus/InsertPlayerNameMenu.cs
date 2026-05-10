using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InsertPlayerNameMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;

    [Header("Scenes")]
    [SerializeField] private string nextSceneName = "HowToPlayScene";
    [SerializeField] private string backSceneName = "CharacterSelectScene";

    [Header("Audio")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private float clickVolume = 1f;

    private bool isLoadingScene;

    private void Start()
    {
        isLoadingScene = false;

        if (nextButton != null)
            nextButton.onClick.AddListener(GoToNextScene);

        if (backButton != null)
            backButton.onClick.AddListener(GoToBackScene);

        if (playerNameInput != null)
            playerNameInput.Select();
    }

    public void GoToNextScene()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        string playerName = playerNameInput != null ? playerNameInput.text : "Player";

        if (PlayerRunData.Instance != null)
            PlayerRunData.Instance.SetPlayerName(playerName);

        SceneManager.LoadScene(nextSceneName);
    }

    public void GoToBackScene()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        SceneManager.LoadScene(backSceneName);
    }

    private void PlayClickSFX()
    {
        if (clickClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(clickClip, clickVolume);
    }
}