using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    private static PauseMenuUI keyboardController;
    private static GameObject sharedPauseButton;
    private static GameObject sharedPauseScreen;
    private static GameObject sharedMenuPause;
    private static GameObject sharedMenuPauseShadow;
    private static GameObject sharedSoundMenu;
    private static Slider sharedMusicVolumeSlider;
    private static Slider sharedSfxVolumeSlider;

    [Header("Pause Elements")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuPauseShadow;

    [Header("Audio")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private float clickVolume = 1f;

    [Header("Sound Settings")]
    [SerializeField] private GameObject soundMenu;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private static bool isPaused;

    private void Awake()
    {
        RegisterSharedReferences();
        ConfigureSoundSliders();
    }

    private void OnEnable()
    {
        RegisterSharedReferences();
        ConfigureSoundSliders();
        TryBecomeKeyboardController();
    }

    private void OnDisable()
    {
        if (keyboardController == this)
            keyboardController = null;

        RemoveSoundSliderListeners();
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

        if (soundMenu != null)
            sharedSoundMenu = soundMenu;

        if (musicVolumeSlider != null)
            sharedMusicVolumeSlider = musicVolumeSlider;

        if (sfxVolumeSlider != null)
            sharedSfxVolumeSlider = sfxVolumeSlider;
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

        ConfigureSoundSliders();
        SyncSettingsSliders();

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

        CloseSettings();

        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        RegisterSharedReferences();
        ConfigureSoundSliders();
        SyncSettingsSliders();

        GameObject targetSoundMenu = soundMenu != null ? soundMenu : sharedSoundMenu;
        GameObject targetMenuPause = menuPause != null ? menuPause : sharedMenuPause;
        GameObject targetMenuPauseShadow = menuPauseShadow != null ? menuPauseShadow : sharedMenuPauseShadow;

        if (targetMenuPause != null)
            targetMenuPause.SetActive(false);

        if (targetMenuPauseShadow != null)
            targetMenuPauseShadow.SetActive(false);

        if (targetSoundMenu != null)
            targetSoundMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        RegisterSharedReferences();

        GameObject targetSoundMenu = soundMenu != null ? soundMenu : sharedSoundMenu;
        GameObject targetMenuPause = menuPause != null ? menuPause : sharedMenuPause;
        GameObject targetMenuPauseShadow = menuPauseShadow != null ? menuPauseShadow : sharedMenuPauseShadow;

        if (targetSoundMenu != null)
            targetSoundMenu.SetActive(false);

        if (!isPaused)
            return;

        if (targetMenuPause != null)
            targetMenuPause.SetActive(true);

        if (targetMenuPauseShadow != null)
            targetMenuPauseShadow.SetActive(true);
    }

    public void GoToMainMenu()
    {
        isPaused = false;
        CloseSettings();
        ResetRunProgress();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToCharacterSelectScene()
    {
        isPaused = false;
        CloseSettings();
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

    public void SetMusicVolume(float volume)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMasterMusicVolume(volume);
    }

    public void SetSfxVolume(float volume)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.SetMasterSfxVolume(volume);
    }

    private void ConfigureSoundSliders()
    {
        RegisterSharedReferences();

        Slider targetMusicSlider = musicVolumeSlider != null ? musicVolumeSlider : sharedMusicVolumeSlider;
        Slider targetSfxSlider = sfxVolumeSlider != null ? sfxVolumeSlider : sharedSfxVolumeSlider;

        if (targetMusicSlider != null)
        {
            targetMusicSlider.minValue = 0f;
            targetMusicSlider.maxValue = 1f;
            targetMusicSlider.wholeNumbers = false;
            targetMusicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            targetMusicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (targetSfxSlider != null)
        {
            targetSfxSlider.minValue = 0f;
            targetSfxSlider.maxValue = 1f;
            targetSfxSlider.wholeNumbers = false;
            targetSfxSlider.onValueChanged.RemoveListener(SetSfxVolume);
            targetSfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }
    }

    private void RemoveSoundSliderListeners()
    {
        Slider targetMusicSlider = musicVolumeSlider != null ? musicVolumeSlider : sharedMusicVolumeSlider;
        Slider targetSfxSlider = sfxVolumeSlider != null ? sfxVolumeSlider : sharedSfxVolumeSlider;

        if (targetMusicSlider != null)
            targetMusicSlider.onValueChanged.RemoveListener(SetMusicVolume);

        if (targetSfxSlider != null)
            targetSfxSlider.onValueChanged.RemoveListener(SetSfxVolume);
    }

    private void SyncSettingsSliders()
    {
        RegisterSharedReferences();
        ConfigureSoundSliders();

        Slider targetMusicSlider = musicVolumeSlider != null ? musicVolumeSlider : sharedMusicVolumeSlider;
        Slider targetSfxSlider = sfxVolumeSlider != null ? sfxVolumeSlider : sharedSfxVolumeSlider;

        if (targetMusicSlider != null)
        {
            targetMusicSlider.minValue = 0f;
            targetMusicSlider.maxValue = 1f;

            if (MusicManager.Instance != null)
                targetMusicSlider.SetValueWithoutNotify(MusicManager.Instance.GetMasterMusicVolume());
        }

        if (targetSfxSlider != null)
        {
            targetSfxSlider.minValue = 0f;
            targetSfxSlider.maxValue = 1f;

            if (SFXManager.Instance != null)
                targetSfxSlider.SetValueWithoutNotify(SFXManager.Instance.GetMasterSfxVolume());
        }
    }
}
