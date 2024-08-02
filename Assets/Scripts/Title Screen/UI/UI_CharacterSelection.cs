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
    public TextMeshProUGUI[] playerNamesText;
    public Button confirmButton;
    public TextMeshProUGUI timerText;

    private int selectedCharacterIndex = -1;
    private float countdown = 10f;
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

    private async void OnCharacterButtonClicked(Button button)
    {
        selectedCharacterIndex = System.Array.IndexOf(characterButtons, button);
        Debug.Log($"Character {selectedCharacterIndex} selected");

        if (selectedCharacterIndex != -1)
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            var lobbyData = new Dictionary<string, DataObject>
            {
                { "characterIndex_" + playerId, new DataObject(DataObject.VisibilityOptions.Member, selectedCharacterIndex.ToString()) }
            };

            if (LobbyManager.Instance.GetHostId() == playerId)
            {
                await LobbyService.Instance.UpdateLobbyAsync(LobbyManager.Instance.GetCurrentLobby().Id, new UpdateLobbyOptions { Data = lobbyData });
            }

            UpdatePlayerStatus();
        }
    }

    private async void OnConfirmButtonClicked()
    {
        if (selectedCharacterIndex != -1)
        {
            Debug.Log($"Character {selectedCharacterIndex} confirmed");

            string playerId = AuthenticationService.Instance.PlayerId;
            var lobbyData = new Dictionary<string, DataObject>
            {
                { "characterIndex_" + playerId, new DataObject(DataObject.VisibilityOptions.Member, selectedCharacterIndex.ToString()) }
            };

            if (LobbyManager.Instance.GetHostId() == playerId)
            {
                await LobbyService.Instance.UpdateLobbyAsync(LobbyManager.Instance.GetCurrentLobby().Id, new UpdateLobbyOptions { Data = lobbyData });
            }

            CheckAllPlayersReady();
            UpdatePlayerStatus();
        }
    }

    public void ShowCharacterSelection()
    {
        characterSelectionPanel.SetActive(true);
        isCountdownActive = false;
        countdown = 10f;
        UpdatePlayerStatus();
    }

    public void StartCountdown()
    {
        isCountdownActive = true;
    }

    private void CheckAllPlayersReady()
    {
        var lobby = LobbyManager.Instance.GetCurrentLobby();
        bool allReady = true;

        foreach (var player in lobby.Players)
        {
            if (!lobby.Data.ContainsKey("characterIndex_" + player.Id))
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            UI_Lobby.Instance.SetAllPlayersReady(true);
            StartGame();
        }
        else
        {
            UI_Lobby.Instance.SetAllPlayersReady(false);
        }
    }

    private void UpdatePlayerStatus()
    {
        var lobby = LobbyManager.Instance.GetCurrentLobby();

        foreach (var player in lobby.Players)
        {
            string playerId = player.Id;

            for (int i = 0; i < characterButtons.Length; i++)
            {
                if (lobby.Data.ContainsKey("characterIndex_" + playerId) && int.Parse(lobby.Data["characterIndex_" + playerId].Value) == i)
                {
                    playerNamesText[i].text = playerId + " (Ready)";
                }
                else if (string.IsNullOrEmpty(playerNamesText[i].text) || playerNamesText[i].text == playerId + " (Ready)")
                {
                    playerNamesText[i].text = "";
                }
            }
        }
    }

    public async void StartGame()
    {
        isCountdownActive = false;
        Debug.Log("Starting game...");
        Debug.Log(selectedCharacterIndex);

        if (LobbyManager.Instance.GetHostId() == AuthenticationService.Instance.PlayerId)
        {
            string joinCode = await GameManager.Instance.StartHostWithRelay();
            Debug.Log($"Host started with join code: {joinCode}");
        }
        else
        {
            string joinCode = LobbyManager.Instance.GetJoinCode();
            if (joinCode != null)
            {
                bool success = await GameManager.Instance.StartClientWithRelay(joinCode);
                Debug.Log($"Client joined with join code: {joinCode}, success: {success}");
            }
            else
            {
                Debug.LogError("Join code not found!");
            }
        }
        SaveGameManager.Instance.singlePlayer = false;
        SaveGameManager.Instance.NewGame(selectedCharacterIndex);
    }

    public async void Leave()
    {
        await LobbyManager.Instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        characterSelectionPanel.SetActive(false);
    }
}
