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
            continueButton.onClick.AddListener(ContinueToConfiguredScene);
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
            ContinueToConfiguredScene();
    }

    // Usa la escena configurada desde el inspector.
    // Sirve para continuar con Space o para botones sin parámetro.
    public void ContinueToConfiguredScene()
    {
        LoadScene(nextLevelSceneName);
    }

    // Este método es el que debes seleccionar desde el OnClick del botón
    // si quieres indicar la escena manualmente desde Unity.
    public void ContinueToScene(string sceneName)
    {
        LoadScene(sceneName);
    }

    private void LoadScene(string sceneName)
    {
        if (isLoadingScene)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[HowToPlay] Scene name is empty.");
            return;
        }

        isLoadingScene = true;

        PlayClickSFX();

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