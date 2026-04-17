using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause Elements")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject menuPause;

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

    public void ExitGame()
    {
        Debug.Log("Ir a Main Menu");
        Time.timeScale = 1f;

        SceneTransitionManager.instance.ClearPlayerCoins();
        SceneTransitionManager.instance.ClearPlayerHealth();

        SceneManager.LoadScene("CharacterSelectScene");
    }
}