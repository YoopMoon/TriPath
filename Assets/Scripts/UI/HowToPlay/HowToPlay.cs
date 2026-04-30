using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HowToPlay : MonoBehaviour
{
    [Header("Continue Button")]
    [SerializeField] private Button continueButton;

    [Header("Next Scene")]
    [SerializeField] private string nextLevelSceneName = "Map1_Level1";

    [Header("Audio")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private float clickVolume = 1f;


    private bool isLoadingScene;

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToGame);
    }

    private void Update()
    {
        HandleContinueInput();
    }

    private void HandleContinueInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            ContinueToGame();
    }

    public void ContinueToGame()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        SceneManager.LoadScene(nextLevelSceneName);
    }

    private void PlayClickSFX()
    {
        if (clickClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(clickClip, clickVolume);
    }
}