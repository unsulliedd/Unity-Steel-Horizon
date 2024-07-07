using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SubmenuControl : MonoBehaviour
{
    [SerializeField] GameObject newGameSubmenu;
    [SerializeField] GameObject startAsHostButton;
    [SerializeField] GameObject joinAsClientButton;
    [SerializeField] GameObject newGameButton;      

    void Update()
    {
        if (PlayerInputManager.Instance.submitPerformed && !PlayerInputManager.Instance.isGamepadActive && !IsPointerOverUIObject())
        {
            startAsHostButton.SetActive(false);
            joinAsClientButton.SetActive(false);

        }

        if (PlayerInputManager.Instance.cancelPerformed)
        {
            startAsHostButton.SetActive(false);
            joinAsClientButton.SetActive(false);
            StartCoroutine(SetFirstSelectedButton(newGameButton));
            
        }
    }

    public void ToggleSubMenu()
    {
        bool isActive = !startAsHostButton.activeSelf;
        startAsHostButton.SetActive(isActive);
        joinAsClientButton.SetActive(isActive);

        if (isActive)
        {
            StartCoroutine(SetFirstSelectedButton(startAsHostButton));
        }
        else
        {
            StartCoroutine(SetFirstSelectedButton(newGameButton));
        }
    }

    private IEnumerator SetFirstSelectedButton(GameObject button)
    {
        yield return null;

        if (button != null)
        {
            EventSystem.current.SetSelectedGameObject(button);
        }
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
