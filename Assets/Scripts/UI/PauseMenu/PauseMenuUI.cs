using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    private static PauseMenuUI keyboardController;
    private static GameObject sharedPauseButton;
    private static GameObject sharedPauseScreen;
    private static GameObject sharedMenuPause;
    private static GameObject sharedMenuPauseShadow;

    [Header("Pause Elements")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuPauseShadow;

    [Header("Audio")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private float clickVolume = 1f;

    private static bool isPaused;

    private void Awake()
    {
        RegisterSharedReferences();
    }

    private void OnEnable()
    {
        RegisterSharedReferences();
        TryBecomeKeyboardController();
    }

    private void OnDisable()
    {
        if (keyboardController == this)
            keyboardController = null;
    }

    private void Update()
    {
        if (keyboardController == null)
            TryBecomeKeyboardController();

        if (keyboardController != this)
            return;

        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (isPaused)
            ResumeGame();
        else
            OpenPauseMenu();
    }

    private void TryBecomeKeyboardController()
    {
        if (keyboardController == null || (keyboardController.pauseButton == null && pauseButton != null))
            keyboardController = this;
    }

    private void RegisterSharedReferences()
    {
        if (pauseButton != null)
            sharedPauseButton = pauseButton;

        if (pauseScreen != null)
            sharedPauseScreen = pauseScreen;

        if (menuPause != null)
            sharedMenuPause = menuPause;

        if (menuPauseShadow != null)
            sharedMenuPauseShadow = menuPauseShadow;
    }

    public void OpenPauseMenu()
    {
        RegisterSharedReferences();
        isPaused = true;

        GameObject targetPauseButton = pauseButton != null ? pauseButton : sharedPauseButton;
        GameObject targetPauseScreen = pauseScreen != null ? pauseScreen : sharedPauseScreen;
        GameObject targetMenuPause = menuPause != null ? menuPause : sharedMenuPause;
        GameObject targetMenuPauseShadow = menuPauseShadow != null ? menuPauseShadow : sharedMenuPauseShadow;

        if (targetPauseButton != null)
            targetPauseButton.SetActive(false);

        if (targetPauseScreen != null)
            targetPauseScreen.SetActive(true);

        if (targetMenuPause != null)
            targetMenuPause.SetActive(true);

        if (targetMenuPauseShadow != null)
            targetMenuPauseShadow.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        RegisterSharedReferences();
        isPaused = false;

        GameObject targetPauseButton = pauseButton != null ? pauseButton : sharedPauseButton;
        GameObject targetPauseScreen = pauseScreen != null ? pauseScreen : sharedPauseScreen;
        GameObject targetMenuPause = menuPause != null ? menuPause : sharedMenuPause;
        GameObject targetMenuPauseShadow = menuPauseShadow != null ? menuPauseShadow : sharedMenuPauseShadow;

        if (targetPauseButton != null)
            targetPauseButton.SetActive(true);

        if (targetPauseScreen != null)
            targetPauseScreen.SetActive(false);

        if (targetMenuPause != null)
            targetMenuPause.SetActive(false);

        if (targetMenuPauseShadow != null)
            targetMenuPauseShadow.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        Debug.Log("Abrir ajustes");
    }

    public void GoToMainMenu()
    {
        isPaused = false;
        ResetRunProgress();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToCharacterSelectScene()
    {
        isPaused = false;
        ResetRunProgress();
        Time.timeScale = 1f;
        SceneManager.LoadScene("CharacterSelectScene");
    }

    private void ResetRunProgress()
    {
        if (AdaptiveDifficultyManager.Instance != null)
            AdaptiveDifficultyManager.Instance.ResetProgress();

        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.ResetProgress();

        if (CollectibleStateManager.Instance != null)
            CollectibleStateManager.Instance.ResetProgress();

        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.ResetProgress();

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.ResetProgress();
    }

    public void PlayClickButton()
    {
        if (clickClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(clickClip, clickVolume);
    }

    public void PauseMusic()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.PauseMusic();
    }

    public void ResumeMusic()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.ResumeMusic();
    }
}
