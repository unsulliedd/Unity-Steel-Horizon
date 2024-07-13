using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TitleScreenManager : MonoBehaviour
{
    #region Title Screen Menu
    [Header("Title Screen Menu")]
    public GameObject titleScreenMenu;
    [SerializeField] GameObject titleScreenLoadMenu;
    #endregion
    #region Submenu
    [Header("Submenu")]
    [SerializeField] GameObject newGameSubmenu;
    [SerializeField] GameObject newGameButton;
    [SerializeField] GameObject startAsHostButton;
    #endregion
    #region Load Menu
    [Header("Load Menu")]
    [SerializeField] GameObject backButtonLoadMenu;

    [Header("Load Menu Delete Info")]
    [SerializeField] TextMeshProUGUI deleteInfoText;
    [SerializeField] GameObject deleteInfoMouseImage;
    [SerializeField] GameObject deleteInfoGamepadImage;
    #endregion
    #region No Empty Slots Panel
    [Header("No Empty Slots Panel")]
    [SerializeField] GameObject noEmptySlotsPanel;
    [SerializeField] GameObject noEmptySlotsPanelButton;
    #endregion
    #region Save File Delete Confirmation Panel
    [Header("Save File Delete Confirmation Panel")]
    [SerializeField] GameObject saveFileDeleteConfirmationPanel;
    [SerializeField] GameObject saveFileDeleteConfirmationPanelText;
    [SerializeField] GameObject saveFileDeleteConfirmationPanelYesButton;
    [SerializeField] GameObject saveFileDeleteConfirmationPanelNoButton;
    #endregion

    public CharacterSlot currentCharacterSlot = CharacterSlot.NO_SLOT;

    public static TitleScreenManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (titleScreenLoadMenu.activeSelf)
        {
            SelectDeleteInfoProvider();
            if ((PlayerInputManager.Instance.deletePerformed || PlayerInputManager.Instance.rightClickPerformed))
                TryShowSaveFileDeleteConfirmationPanel();
        }

        if (PlayerInputManager.Instance.cancelPerformed)
        {
            HandleCancellation();
        }
    }

    private void HandleCancellation()
    {
        var panels = new List<(GameObject panel, GameObject button, GameObject fallbackPanel)>
        {
            (newGameSubmenu, newGameButton, titleScreenMenu),
            (titleScreenLoadMenu, backButtonLoadMenu, titleScreenMenu),
            (noEmptySlotsPanel, noEmptySlotsPanelButton, titleScreenMenu),
            (saveFileDeleteConfirmationPanel, backButtonLoadMenu, titleScreenLoadMenu)
        };

        foreach (var (panel, button, fallbackPanel) in panels)
        {
            if (panel.activeSelf)
            {
                panel.SetActive(false);
                fallbackPanel.SetActive(true);
                StartCoroutine(SetFirstSelectedButton(button));
                return;
            }
        }
    }

    public void StartNetworkAsHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartNetworkAsClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void ShutdownServer()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public void StartNewGame()
    {
        SaveGameManager.Instance.NewGame();
    }

    public void LoadGame()
    {
        SaveGameManager.Instance.LoadGame();
        StartNetworkAsHost();
    }

    public void ToggleSubMenu()
    {
        bool isActive = !newGameSubmenu.activeSelf;
        newGameSubmenu.SetActive(isActive);

        if (isActive)
            StartCoroutine(SetFirstSelectedButton(startAsHostButton));
        else
            StartCoroutine(SetFirstSelectedButton(newGameButton));
    }

    public void SelectCharacterSlot(CharacterSlot slot)
    {
        currentCharacterSlot = slot;
    }

    public void SelectNoSlot()
    {
        currentCharacterSlot = CharacterSlot.NO_SLOT;
    }

    // Open load menu
    public void OpenLoadMenu()
    {
        titleScreenLoadMenu.SetActive(true);
        titleScreenMenu.SetActive(false);
        StartCoroutine(SetFirstSelectedButton(backButtonLoadMenu));
    }

    // Close load menu
    public void CloseLoadMenu()
    {
        titleScreenMenu.SetActive(true);
        titleScreenLoadMenu.SetActive(false);
        StartCoroutine(SetFirstSelectedButton(newGameButton));
    }

    public void NoEmptySlotPanelPopUp()
    {
        noEmptySlotsPanel.SetActive(true);
        StartCoroutine(SetFirstSelectedButton(noEmptySlotsPanelButton));
    }

    public void TryShowSaveFileDeleteConfirmationPanel()
    {
        if (SaveGameManager.Instance.IsSlotEmpty(currentCharacterSlot))
        {
            Debug.Log("Cannot delete an empty slot.");
            return;
        }
        SaveFileDeleteConfirmationPanelPopUp();
    }

    public void SaveFileDeleteConfirmationPanelPopUp()
    {
        saveFileDeleteConfirmationPanel.SetActive(true);
        StartCoroutine(SetFirstSelectedButton(saveFileDeleteConfirmationPanelNoButton));
        SaveFileConfirmationEditText();
    }

    public void SaveFileDeleteConfirmationPanelNoButton()
    {
        saveFileDeleteConfirmationPanel.SetActive(false);
        StartCoroutine(SetFirstSelectedButton(backButtonLoadMenu));
    }

    public void SaveFileDeleteConfirmationPanelYesButton()
    {
        SaveGameManager.Instance.DeleteSaveGame(currentCharacterSlot);
        saveFileDeleteConfirmationPanel.SetActive(false);
        titleScreenLoadMenu.SetActive(false);
        titleScreenLoadMenu.SetActive(true);
        StartCoroutine(SetFirstSelectedButton(backButtonLoadMenu));
    }

    public void SaveFileConfirmationEditText()
    {
        saveFileDeleteConfirmationPanelText.GetComponent<TMPro.TextMeshProUGUI>().text = "Are you sure you want to delete " + SaveGameManager.Instance.AssignFileNamebyCharacterSlot(currentCharacterSlot) + "?";
    }

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

    // Quit game
    public void QuitGame()
    {
        Application.Quit();
    }

    // Set the first selected button
    public IEnumerator SetFirstSelectedButton(GameObject button)
    {
        yield return null;
        if (button != null)
            EventSystem.current.SetSelectedGameObject(button);
    }
}
