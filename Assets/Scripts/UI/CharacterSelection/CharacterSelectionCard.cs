using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSelectionCard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CharacterSelectionMenu selectionMenu;
    [SerializeField] private int characterIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectionMenu != null)
            selectionMenu.SelectCharacter(characterIndex);
    }
}