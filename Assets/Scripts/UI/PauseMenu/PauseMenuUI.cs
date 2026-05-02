using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause Elements")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject menuPause;

    [Header("Audio")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private float clickVolume = 1f;

    public void OpenPauseMenu()
    {
        if (pauseButton != null)
            pauseButton.SetActive(false);

        if (pauseScreen != null)
            pauseScreen.SetActive(true);

        if (menuPause != null)
            menuPause.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (pauseButton != null)
            pauseButton.SetActive(true);

        if (pauseScreen != null)
            pauseScreen.SetActive(false);

        if (menuPause != null)
            menuPause.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        Debug.Log("Abrir ajustes");
    }

    public void ExitGame() // Irá al menú principal
    {
        ResetRunProgress();

        Time.timeScale = 1f;
        SceneManager.LoadScene("CharacterSelectScene");
    }

    public void ChangeCharacter()
    {
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