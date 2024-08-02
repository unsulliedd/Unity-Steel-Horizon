using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the transition from the title screen to the main menu upon any key press.
/// </summary>
public class UI_Button_AnyKey : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject ugsPanel;
    [SerializeField] private GameObject newGameButton;

    private void Update()
    {
        CheckForAnyKeyPressed();
    }

    /// <summary>
    /// Checks if any key is pressed and handles the transition to the main menu.
    /// </summary>
    private void CheckForAnyKeyPressed()
    {
        if (PlayerInputManager.Instance.anyKeyPerformed || Input.GetMouseButtonDown(0))
        {
            EventSystem.current.SetSelectedGameObject(newGameButton);
            titleScreen.SetActive(false);
            ugsPanel.SetActive(true);
        }
    }
}
