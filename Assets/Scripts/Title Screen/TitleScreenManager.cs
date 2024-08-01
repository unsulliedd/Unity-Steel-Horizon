using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the title screen and related UI functionalities.
/// </summary>
public class TitleScreenManager : MonoBehaviour
{
    #region Title Screen Menu
    [Header("Title Screen Menu")]
    public GameObject titleScreenMenu;                              // Main title screen menu
    [SerializeField] private GameObject titleScreenLoadMenu;        // Load menu screen
    public GameObject titleScreenBackground;        
    #endregion

    #region Submenu
    [Header("Submenu")]
    [SerializeField] private GameObject newGameSubmenu;             // New game submenu
    [SerializeField] private GameObject newGameButton;              // New game button
    [SerializeField] private GameObject loadGameButton;             // Load game button
    #endregion

    #region Profile
    [Header("Profile")]
    [SerializeField] private TextMeshProUGUI profileName;           // Profile button
    [SerializeField] private TextMeshProUGUI profileLevel;          // Profile button
    [SerializeField] private Image profilePicture;                  // Profile picture
    #endregion

    #region Load Menu
    [Header("Load Menu")]
    [SerializeField] private GameObject backButtonLoadMenu;         // Back button in the load menu

    [Header("Load Menu Delete Info")]
    [SerializeField] private TextMeshProUGUI deleteInfoText;        // Text for delete information
    [SerializeField] private GameObject deleteInfoMouseImage;       // Image for mouse delete info
    [SerializeField] private GameObject deleteInfoGamepadImage;     // Image for gamepad delete info
    #endregion

    [Header("Lobby")]
    [SerializeField] private GameObject lobbyPanel;                 // Lobby panel

    #region No Empty Slots Panel
    [Header("No Empty Slots Panel")]
    [SerializeField] private GameObject noEmptySlotsPanel;          // Panel for no empty slots
    [SerializeField] private GameObject noEmptySlotsPanelButton;    // Button on no empty slots panel
    #endregion

    #region Save File Delete Confirmation Panel
    [Header("Save File Delete Confirmation Panel")]
    [SerializeField] private GameObject saveFileDeleteConfirmationPanel;            // Panel for save file delete confirmation
    [SerializeField] private GameObject saveFileDeleteConfirmationPanelText;        // Text on save file delete confirmation panel
    [SerializeField] private GameObject saveFileDeleteConfirmationPanelYesButton;   // Yes button on save file delete confirmation panel
    [SerializeField] private GameObject saveFileDeleteConfirmationPanelNoButton;    // No button on save file delete confirmation panel
    #endregion

    public CharacterSlot currentCharacterSlot = CharacterSlot.NO_SLOT; // Current selected character slot
    public bool startAsHost = false; // Flag to start as host
    public bool startAsClient = false; // Flag to start as client

    #region Singleton
    public static TitleScreenManager Instance { get; private set; } // Singleton instance of TitleScreenManager
    #endregion

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private async void Start()
    {
        if (SteamManager.Instance.isSteamRunning)
        {
            profileName.text = SteamManager.Instance.GetSteamUserName();
            string level = SteamManager.Instance.GetSteamLevel();
            profileLevel.text = $"Level: {level}";
            var avatar = await SteamManager.Instance.GetSteamUserAvatar();
            if (avatar != null)
            {
                profilePicture.sprite = avatar;
            }
        }
    }

    private void Update()
    {
        if (titleScreenLoadMenu.activeSelf)
        {
            // Update delete info based on input type
            SelectDeleteInfoProvider();
            if (PlayerInputManager.Instance.deletePerformed || PlayerInputManager.Instance.rightClickPerformed)
                TryShowSaveFileDeleteConfirmationPanel();
        }

        if (PlayerInputManager.Instance.cancelPerformed)
            HandleCancellation();
    }

    public void StartSinglePLayerNewGame()
    {
        //SaveGameManager.Instance.NewGame();
    }

    /// <summary>
    /// Handles the cancellation action based on the active panel.
    /// </summary>
    private void HandleCancellation()
    {
        // List of panels and their corresponding buttons and fallback panels
        var panels = new List<(GameObject panel, GameObject button, GameObject fallbackPanel)>
        {
            (newGameSubmenu, newGameButton, titleScreenMenu),
            (titleScreenLoadMenu, newGameButton, titleScreenMenu),
            (noEmptySlotsPanel, newGameButton, titleScreenMenu),
            (saveFileDeleteConfirmationPanel, backButtonLoadMenu, titleScreenLoadMenu)
        };
        // Check which panel is active and switch to the fallback panel
        foreach (var (panel, button, fallbackPanel) in panels)
        {
            if (panel.activeSelf)
            {
                Debug.Log(panel);
                Debug.Log($"{button} {fallbackPanel}");
                panel.SetActive(false);
                fallbackPanel.SetActive(true);
                StartCoroutine(SetFirstSelectedButton(button));
                return;
            }
        }
    }

    /// <summary>
    /// Starts the network as host.
    /// </summary>
    public void StartNetworkAsHost() => startAsHost = true;

    /// <summary>
    /// Starts the network as client.
    /// </summary>
    public void StartNetworkAsClient() => startAsClient = true;

    /// <summary>
    /// Shuts down the server.
    /// </summary>
    public void ShutdownServer()
    {
        NetworkManager.Singleton.Shutdown();
    }

    /// <summary>
    /// Starts a new game.
    /// </summary>
    public void StartNewGame()
    {
        //SaveGameManager.Instance.NewGame();
    }

    /// <summary>
    /// Loads the game.
    /// </summary>
    public void LoadGame()
    {
        SaveGameManager.Instance.LoadGame();
    }

    /// <summary>
    /// Toggles the new game submenu.
    /// </summary>
    public void ToggleSubMenu()
    {
        bool isActive = !newGameSubmenu.activeSelf;
        newGameSubmenu.SetActive(isActive);

        if (isActive)
            StartCoroutine(SetFirstSelectedButton(loadGameButton));
        else
            StartCoroutine(SetFirstSelectedButton(newGameButton));
    }

    /// <summary>
    /// Selects the character slot.
    /// </summary>
    /// <param name="slot">The character slot to select.</param>
    public void SelectCharacterSlot(CharacterSlot slot)
    {
        currentCharacterSlot = slot;
    }

    /// <summary>
    /// Selects no slot.
    /// </summary>
    public void SelectNoSlot()
    {
        currentCharacterSlot = CharacterSlot.NO_SLOT;
    }

    /// <summary>
    /// Opens the load menu.
    /// </summary>
    public void OpenLoadMenu()
    {
        titleScreenLoadMenu.SetActive(true);
        titleScreenMenu.SetActive(false);
        StartCoroutine(SetFirstSelectedButton(backButtonLoadMenu));
    }

    /// <summary>
    /// Closes the load menu.
    /// </summary>
    public void CloseLoadMenu()
    {
        titleScreenMenu.SetActive(true);
        titleScreenLoadMenu.SetActive(false);
        StartCoroutine(SetFirstSelectedButton(newGameButton));
    }

    public void LobbyMenuToggle()
    {
        lobbyPanel.SetActive(!lobbyPanel.activeSelf);
    }

    /// <summary>
    /// Shows the no empty slots panel.
    /// </summary>
    public void NoEmptySlotPanelPopUp()
    {
        noEmptySlotsPanel.SetActive(true);
        StartCoroutine(SetFirstSelectedButton(noEmptySlotsPanelButton));
    }

    /// <summary>
    /// Tries to show the save file delete confirmation panel.
    /// </summary>
    public void TryShowSaveFileDeleteConfirmationPanel()
    {
        if (SaveGameManager.Instance.IsSlotEmpty(currentCharacterSlot))
        {
            Debug.Log("Cannot delete an empty slot.");
            return;
        }
        SaveFileDeleteConfirmationPanelPopUp();
    }

    /// <summary>
    /// Shows the save file delete confirmation panel.
    /// </summary>
    public void SaveFileDeleteConfirmationPanelPopUp()
    {
        saveFileDeleteConfirmationPanel.SetActive(true);
        StartCoroutine(SetFirstSelectedButton(saveFileDeleteConfirmationPanelNoButton));
        SaveFileConfirmationEditText();
    }

    /// <summary>
    /// Hides the save file delete confirmation panel.
    /// </summary>
    public void SaveFileDeleteConfirmationPanelNoButton()
    {
        saveFileDeleteConfirmationPanel.SetActive(false);
        StartCoroutine(SetFirstSelectedButton(backButtonLoadMenu));
    }

    /// <summary>
    /// Deletes the save file and updates the UI.
    /// </summary>
    public void SaveFileDeleteConfirmationPanelYesButton()
    {
        SaveGameManager.Instance.DeleteSaveGame(currentCharacterSlot);
        saveFileDeleteConfirmationPanel.SetActive(false);
        titleScreenLoadMenu.SetActive(false);
        titleScreenLoadMenu.SetActive(true); // Refresh the load menu
        StartCoroutine(SetFirstSelectedButton(backButtonLoadMenu));
    }

    /// <summary>
    /// Updates the text on the save file delete confirmation panel.
    /// </summary>
    public void SaveFileConfirmationEditText()
    {
        saveFileDeleteConfirmationPanelText.GetComponent<TMPro.TextMeshProUGUI>().text = "Are you sure you want to delete " + SaveGameManager.Instance.AssignFileNamebyCharacterSlot(currentCharacterSlot) + "?";
    }

    /// <summary>
    /// Selects the appropriate delete info provider based on the input type.
    /// </summary>
    private void SelectDeleteInfoProvider()
    {
        if (PlayerInputManager.Instance.isGamepadActive)
        {
            deleteInfoText.text = "Press X button to delete";
            deleteInfoMouseImage.SetActive(false);
            deleteInfoGamepadImage.SetActive(true);
        }
        else
        {
            deleteInfoText.text = "Press right click button to delete";
            deleteInfoGamepadImage.SetActive(false);
            deleteInfoMouseImage.SetActive(true);
        }
    }

    /// <summary>
    /// Quits the game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Sets the first selected button in the UI.
    /// </summary>
    public IEnumerator SetFirstSelectedButton(GameObject button)
    {
        yield return null;
        if (button != null)
            EventSystem.current.SetSelectedGameObject(button);
    }
}
