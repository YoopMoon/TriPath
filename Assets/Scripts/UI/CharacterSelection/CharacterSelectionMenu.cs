using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionMenu : MonoBehaviour
{
    [System.Serializable]
    public class CharacterSlot
    {
        public PlayerSelect.Player playerType;
        public GameObject normalPanel;
        public GameObject selectedPanel;
    }

    [Header("Character Slots")]
    [SerializeField] private CharacterSlot[] characterSlots;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;

    [Header("Scenes")]
    [SerializeField] private string nextSceneName = "InsertPlayerNameScene";
    [SerializeField] private string backSceneName = "MainMenu";

    [Header("Initial Selection")]
    [SerializeField] private int selectedIndex = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip selectionClip;
    [SerializeField] private float selectionVolume = 0.25f;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private float clickVolume = 1f;

    private bool isLoadingScene;

    private void Start()
    {
        isLoadingScene = false;

        if (characterSlots == null || characterSlots.Length == 0)
        {
            Debug.LogWarning("No hay personajes configurados en CharacterSelectionMenu.");
            return;
        }

        UpdateVisualSelection();

        if (nextButton != null)
            nextButton.onClick.AddListener(GoToNextScene);

        if (backButton != null)
            backButton.onClick.AddListener(GoToBackScene);
    }

    private void Update()
    {
        HandleKeyboardSelection();
        HandleNextInput();
    }

    private void HandleKeyboardSelection()
    {
        if (Keyboard.current == null || characterSlots == null || characterSlots.Length == 0)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            ChangeSelection(-1);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            ChangeSelection(1);
        }
    }

    private void HandleNextInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            GoToNextScene();
    }

    private void ChangeSelection(int direction)
    {
        int previousIndex = selectedIndex;

        selectedIndex += direction;

        if (selectedIndex < 0)
            selectedIndex = characterSlots.Length - 1;
        else if (selectedIndex >= characterSlots.Length)
            selectedIndex = 0;

        if (selectedIndex != previousIndex)
            PlaySelectionSFX();

        UpdateVisualSelection();
    }

    private void UpdateVisualSelection()
    {
        for (int i = 0; i < characterSlots.Length; i++)
        {
            bool isSelected = i == selectedIndex;

            if (characterSlots[i].normalPanel != null)
                characterSlots[i].normalPanel.SetActive(!isSelected);

            if (characterSlots[i].selectedPanel != null)
                characterSlots[i].selectedPanel.SetActive(isSelected);
        }

        SelectedPlayerStore.SelectedPlayer = characterSlots[selectedIndex].playerType;
    }

    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= characterSlots.Length)
            return;

        if (selectedIndex != index)
            PlaySelectionSFX();

        selectedIndex = index;
        UpdateVisualSelection();
    }

    public void GoToNextScene()
    {
        if (isLoadingScene)
            return;

        if (characterSlots == null || characterSlots.Length == 0)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        SelectedPlayerStore.SelectedPlayer = characterSlots[selectedIndex].playerType;

        if (SceneTransitionManager.instance != null)
        {
            SceneTransitionManager.instance.ClearPlayerCoins();
            SceneTransitionManager.instance.ClearPlayerHealth();
            SceneTransitionManager.instance.ClearTransitionData();
        }

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

    private void PlaySelectionSFX()
    {
        if (selectionClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(selectionClip, selectionVolume);
    }

    private void PlayClickSFX()
    {
        if (clickClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(clickClip, clickVolume);
    }
}