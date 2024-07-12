using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] GameObject titleScreenMenu;
    [SerializeField] GameObject titleScreenLoadMenu;

    [SerializeField] GameObject backButtonLoadMenu;
    [SerializeField] GameObject NewGameButton;

    [SerializeField] GameObject noEmptySlotsPanel;
    [SerializeField] GameObject noEmptySlotsPanelButton;


    public CharacterSlot currentCharacterSlot = CharacterSlot.NO_SLOT;

    public static TitleScreenManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        StartCoroutine(SetFirstSelectedButton(NewGameButton));
    }

    public void NoEmptySlotPanelPopUp()
    {
        noEmptySlotsPanel.SetActive(true);
        StartCoroutine(SetFirstSelectedButton(noEmptySlotsPanelButton));
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
