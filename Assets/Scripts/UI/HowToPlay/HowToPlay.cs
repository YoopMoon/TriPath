using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HowToPlay : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;

    [Header("Scenes")]
    [SerializeField] private string nextSceneName = "Map1_Level1";
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
    }

    private void Update()
    {
        HandleNextInput();
    }

    private void HandleNextInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            GoToNextScene();
    }

    public void GoToNextScene()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        LoadScene(nextSceneName);
    }

    public void GoToBackScene()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        LoadScene(backSceneName);
    }

    public void GoToScene(string sceneName)
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        LoadScene(sceneName);
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[HowToPlay] Scene name is empty.");
            isLoadingScene = false;
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void PlayClickSFX()
    {
        if (clickClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(clickClip, clickVolume);
    }
}