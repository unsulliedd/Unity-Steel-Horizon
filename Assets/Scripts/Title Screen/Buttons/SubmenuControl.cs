using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SubmenuControl : MonoBehaviour
{
    [SerializeField] GameObject newGameSubmenu;
    [SerializeField] GameObject newGameButton;      
    [SerializeField] GameObject startAsHostButton;

    void Update()
    {
        if (TitleScreenManager.Instance.titleScreenMenu.activeSelf)
        {
            if (PlayerInputManager.Instance.clickPerformed && !IsPointerOverUIObject())
            {
                newGameSubmenu.SetActive(false);
            }
        }
        else
            return;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
