using UnityEngine;

/// <summary>
/// Manages the player's menu, including character, inventory, and skill menus.
/// </summary>
public class PlayerMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject characterMenu;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject skillMenu;
    [SerializeField] private GameObject menuPanel;

    private void Update()
    {
        HandleMenuToggle();
        ToggleInputsBasedOnMenuState();
    }

    /// <summary>
    /// Enables or disables player inputs based on the active state of the menu panel.
    /// </summary>
    private void ToggleInputsBasedOnMenuState()
    {
        if (menuPanel.activeSelf)
        {
            PlayerInputManager.Instance.playerControls.PlayerCamera.Disable();
            PlayerInputManager.Instance.playerControls.PlayerCombat.Disable();
        }
        else
        {
            PlayerInputManager.Instance.playerControls.PlayerCamera.Enable();
            PlayerInputManager.Instance.playerControls.PlayerCombat.Enable();
        }
    }

    /// <summary>
    /// Handles the toggling of menus based on player input.
    /// </summary>
    private void HandleMenuToggle()
    {
        if (PlayerInputManager.Instance.openCharacterMenuInput)
        {
            ToggleMenu(characterMenu);
            PlayerInputManager.Instance.openCharacterMenuInput = false;
        }

        if (PlayerInputManager.Instance.openInventoryMenuInput)
        {
            ToggleMenu(inventoryMenu);
            PlayerInputManager.Instance.openInventoryMenuInput = false;
        }

        if (PlayerInputManager.Instance.openSkillMenuInput)
        {
            ToggleMenu(skillMenu);
            PlayerInputManager.Instance.openSkillMenuInput = false;
        }
    }

    /// <summary>
    /// Toggles the active state of a specified menu and updates the menu panel's state.
    /// </summary>
    private void ToggleMenu(GameObject menu)
    {
        bool isMenuActive = menu.activeSelf;
        CloseAllMenus();
        menu.SetActive(!isMenuActive);
        menuPanel.SetActive(!isMenuActive);
    }

    /// <summary>
    /// Closes all menus.
    /// </summary>
    public void CloseAllMenus()
    {
        characterMenu.SetActive(false);
        inventoryMenu.SetActive(false);
        skillMenu.SetActive(false);
        menuPanel.SetActive(false);
    }

    /// <summary>
    /// Toggles the character menu.
    /// </summary>
    public void InfoMenuButton() => ToggleMenu(characterMenu);

    /// <summary>
    /// Toggles the inventory menu.
    /// </summary>
    public void InventoryMenuButton() => ToggleMenu(inventoryMenu);

    /// <summary>
    /// Toggles the skill menu.
    /// </summary>
    public void SkillMenuButton() => ToggleMenu(skillMenu);
}
