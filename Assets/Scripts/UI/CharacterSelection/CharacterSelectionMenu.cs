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

    [Header("Continue Button")]
    [SerializeField] private Button continueButton;

    [Header("Next Scene")]
    [SerializeField] private string nextLevelSceneName = "HowToPlay";

    [Header("Initial Selection")]
    [SerializeField] private int selectedIndex = 1;

    private void Start()
    {
        if (characterSlots == null || characterSlots.Length == 0)
        {
            Debug.LogWarning("No hay personajes configurados en CharacterSelectionMenu.");
            return;
        }

        UpdateVisualSelection();

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToGame);
    }

    private void Update()
    {
        HandleKeyboardSelection();
        HandleContinueInput();
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

    private void HandleContinueInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            ContinueToGame();
        }
    }

    private void ChangeSelection(int direction)
    {
        selectedIndex += direction;

        if (selectedIndex < 0)
            selectedIndex = characterSlots.Length - 1;
        else if (selectedIndex >= characterSlots.Length)
            selectedIndex = 0;

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

        // Guardo ya el personaje actual mientras se va navegando
        SelectedPlayerStore.SelectedPlayer = characterSlots[selectedIndex].playerType;
    }

    public void SelectCharacter(int index)
    {
        if (index < 0 || index >= characterSlots.Length)
            return;

        selectedIndex = index;
        UpdateVisualSelection();
    }

    private bool isLoadingScene;

    public void ContinueToGame()
    {
        if (isLoadingScene)
            return;

        if (characterSlots == null || characterSlots.Length == 0)
            return;

        isLoadingScene = true;

        SelectedPlayerStore.SelectedPlayer = characterSlots[selectedIndex].playerType;

        if (SceneTransitionManager.instance != null)
        {
            SceneTransitionManager.instance.ClearPlayerCoins();
            SceneTransitionManager.instance.ClearPlayerHealth();
            SceneTransitionManager.instance.ClearTransitionData();
        }

        SceneManager.LoadScene(nextLevelSceneName);
    }

}