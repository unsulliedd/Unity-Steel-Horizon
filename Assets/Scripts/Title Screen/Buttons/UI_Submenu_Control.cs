using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the visibility of the new game submenu based on player input.
/// </summary>
public class UI_Submenu_Control : MonoBehaviour
{
    [SerializeField] private GameObject newGameSubmenu;
    [SerializeField] private GameObject newGameButton;
    [SerializeField] private GameObject startAsHostButton;

    private void Update()
    {
        if (TitleScreenManager.Instance.titleScreenMenu.activeSelf)
        {
            if (PlayerInputManager.Instance.clickPerformed && !IsPointerOverUIObject())
            {
                newGameSubmenu.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Checks if the pointer is over a UI object.
    /// </summary>
    /// <returns>True if the pointer is over a UI object, false otherwise.</returns>
    private bool IsPointerOverUIObject()
    {
        var eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
