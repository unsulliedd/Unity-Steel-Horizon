using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies;

public class UI_CharacterSelection : MonoBehaviour
{
    public static UI_CharacterSelection Instance { get; private set; }

    public GameObject characterSelectionPanel;
    public Button[] characterButtons;
    public Button confirmButton;
    public TextMeshProUGUI timerText;

    private int selectedCharacterIndex = -1;
    private float countdown = 120f; // Timer süresi saniye olarak
    private bool isCountdownActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (var button in characterButtons)
        {
            button.onClick.AddListener(() => OnCharacterButtonClicked(button));
        }
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        characterSelectionPanel.SetActive(false);
    }

    private void Update()
    {
        if (isCountdownActive)
        {
            countdown -= Time.deltaTime;
            timerText.text = $"Game starts in: {Mathf.Ceil(countdown)}s";
            if (countdown <= 0)
            {
                StartGame();
            }
        }
    }

    private void OnCharacterButtonClicked(Button button)
    {
        selectedCharacterIndex = System.Array.IndexOf(characterButtons, button);
        Debug.Log($"Character {selectedCharacterIndex} selected");
    }

    private async void OnConfirmButtonClicked()
    {
        if (selectedCharacterIndex != -1)
        {
            Debug.Log($"Character {selectedCharacterIndex} confirmed");

            var lobbyData = new Dictionary<string, DataObject>
            {
                { "characterIndex_" + AuthenticationService.Instance.PlayerId, new DataObject(DataObject.VisibilityOptions.Member, selectedCharacterIndex.ToString()) }
            };

            await LobbyService.Instance.UpdateLobbyAsync(LobbyManager.Instance.GetCurrentLobby().Id, new UpdateLobbyOptions { Data = lobbyData });

            StartGame();
        }
    }

    public void ShowCharacterSelection()
    {
        characterSelectionPanel.SetActive(true);
        isCountdownActive = false;
        countdown = 10f; // Zamanlayýcýyý sýfýrla
    }

    public void StartCountdown()
    {
        isCountdownActive = true;
    }

    private void StartGame()
    {
        isCountdownActive = false;
        Debug.Log("Starting game...");


        Debug.Log(selectedCharacterIndex);
        SaveGameManager.Instance.NewGame(selectedCharacterIndex);
        
    }

    public async void Leave()
    {
        await LobbyManager.Instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        characterSelectionPanel.SetActive(false);
    }
}
