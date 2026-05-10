using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pressAnyKeyMessage;
    [SerializeField] private GameObject buttonsContainer;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.5f;

    [Header("Scenes")]
    [SerializeField] private string characterSelectSceneName = "CharacterSelectScene";

    [Header("Audio")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private float clickVolume = 1f;

    private bool menuActivated;
    private bool isLoadingScene;
    private Coroutine blinkCoroutine;

    private void Start()
    {
        menuActivated = false;
        isLoadingScene = false;

        if (buttonsContainer != null)
            buttonsContainer.SetActive(false);

        if (pressAnyKeyMessage != null)
        {
            pressAnyKeyMessage.SetActive(true);
            blinkCoroutine = StartCoroutine(BlinkPressAnyKeyMessage());
        }

        if (playButton != null)
            playButton.onClick.AddListener(Play);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    private void Update()
    {
        if (menuActivated)
            return;

        if (AnyInputPressed())
            ShowMenuButtons();
    }

    private bool AnyInputPressed()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            return true;

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            return true;

        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            return true;

        return false;
    }

    private IEnumerator BlinkPressAnyKeyMessage()
    {
        while (!menuActivated)
        {
            if (pressAnyKeyMessage != null)
                pressAnyKeyMessage.SetActive(!pressAnyKeyMessage.activeSelf);

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void ShowMenuButtons()
    {
        menuActivated = true;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (pressAnyKeyMessage != null)
            pressAnyKeyMessage.SetActive(false);

        if (buttonsContainer != null)
            buttonsContainer.SetActive(true);
    }

    public void Play()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        SceneManager.LoadScene(characterSelectSceneName);
    }

    public void ExitGame()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void PlayClickSFX()
    {
        if (clickClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(clickClip, clickVolume);
    }
}