using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using Unity.Services.Lobbies;

public class UI_Lobby : MonoBehaviour
{
    public static UI_Lobby Instance { get; private set; }

    public Button createLobbyButton;
    public Button listLobbiesButton;
    public TMP_InputField lobbyNameInputField;
    public TMP_InputField lobbyDescriptionInputField;
    public TMP_Dropdown maxPlayersDropdown;
    public TextMeshProUGUI joinCode;
    public Transform lobbyListParent;
    public GameObject lobbyListItemPrefab;

    public GameObject createLobbyPanel;
    public GameObject lobbyDetailsPanel;
    public TextMeshProUGUI lobbyNameText;
    public Transform playersListParent;
    public GameObject playerListItemPrefab;

    public Button startGameButton; // Start game button
    public Lobby currentLobby;

    void Awake()
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

    void Start()
    {
        createLobbyButton.onClick.AddListener(async () => await CreateLobby());
        listLobbiesButton.onClick.AddListener(async () => await ListLobbies());

        maxPlayersDropdown.value = 3;

        startGameButton.onClick.AddListener(StartGame); // Start game button click listener
    }

    private async void OnEnable()
    {
        await LobbyManager.Instance.SubscribeLobbyEventsOnLobbyMenu();
    }

    private async Task CreateLobby()
    {
        string lobbyName = lobbyNameInputField.text;
        string lobbyDescription = lobbyDescriptionInputField.text;
        int maxPlayers = int.Parse(maxPlayersDropdown.options[maxPlayersDropdown.value].text);

        if (string.IsNullOrEmpty(lobbyName))
        {
            Debug.LogError("Lobby name cannot be empty.");
            return;
        }

        if (string.IsNullOrEmpty(lobbyDescription))
        {
            lobbyDescription = " ";
        }

        Lobby lobby = await LobbyManager.Instance.CreateLobby(lobbyName, lobbyDescription, maxPlayers);
        if (lobby != null)
        {
            currentLobby = lobby;
            Debug.Log("Lobby successfully created.");
        }

        joinCode.text = RelayManager.Instance.joinCode;
        Debug.Log($"Join Code: {joinCode.text}");

        await ListLobbies();
    }

    private async Task ListLobbies()
    {
        if (LobbyManager.Instance == null)
        {
            Debug.LogError("LobbyManager.Instance is null.");
            return;
        }

        Debug.Log("Fetching lobby list...");
        var lobbies = await LobbyManager.Instance.ListLobbies();
        if (lobbies == null)
        {
            Debug.LogError("ListLobbies returned null.");
            return;
        }

        if (lobbyListParent == null)
        {
            Debug.LogError("lobbyListParent is null.");
            return;
        }

        for (int i = 1; i < lobbyListParent.childCount; i++)
        {
            Destroy(lobbyListParent.GetChild(i).gameObject);
        }

        currentLobby = LobbyManager.Instance.GetCurrentLobby();

        foreach (var lobby in lobbies)
        {
            if (lobbyListItemPrefab == null)
            {
                Debug.LogError("lobbyListItemPrefab is not assigned.");
                return;
            }

            GameObject listItem = Instantiate(lobbyListItemPrefab, lobbyListParent);
            var lobbyListItem = listItem.GetComponent<UI_LobbyListItem>();
            if (lobbyListItem == null)
            {
                Debug.LogError("LobbyListItem component not found on lobbyListItemPrefab.");
                return;
            }

            string description = "No Description";
            if (lobby.Data != null && lobby.Data.ContainsKey("description"))
            {
                description = lobby.Data["description"].Value;
            }
            else
            {
                Debug.LogWarning($"Lobby {lobby.Name} has no description.");
            }

            lobbyListItem.SetLobbyInfo(lobby.Name, description, lobby.Players.Count, lobby.MaxPlayers);

            Button joinButton = lobbyListItem.GetComponentInChildren<Button>();
            joinButton.onClick.AddListener(async () =>
            {
                await LobbyManager.Instance.LeaveLobby();
                await LobbyManager.Instance.JoinLobby(lobby.Id);
                currentLobby = lobby;
                UpdateLobbyDetails(lobby);
            });

            if (currentLobby != null && currentLobby.Id == lobby.Id)
            {
                joinButton.interactable = false;
                UpdateLobbyDetails(lobby);
            }
            else
            {
                joinButton.interactable = true;
            }
        }
    }

    public void UpdateLobbyDetails(Lobby lobby)
    {
        createLobbyPanel.SetActive(false);
        lobbyDetailsPanel.SetActive(true);
        lobbyNameText.text = $"Lobby Name: {lobby.Name}";

        foreach (Transform child in playersListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var player in lobby.Players)
        {
            GameObject playerItem = Instantiate(playerListItemPrefab, playersListParent);
            var playerListItem = playerItem.GetComponent<UI_LobbyPlayerListItem>();
            playerListItem.SetPlayerInfo(player.Id);
        }

        // Start button visibility based on host
        startGameButton.gameObject.SetActive(LobbyManager.Instance.GetHostId() == AuthenticationService.Instance.PlayerId);
    }

    public void ClearLobbyDetails()
    {
        createLobbyPanel.SetActive(true);
        lobbyDetailsPanel.SetActive(false);
    }

    public async void ExitLobby()
    {
        await LobbyManager.Instance.LeaveLobby();
        ClearLobbyDetails();
        listLobbiesButton.onClick.Invoke();
    }

    private async void StartGame()
    {
        if (currentLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                foreach (var player in currentLobby.Players)
                {
                    Debug.Log($"Player {player.Id} is ready");
                    var lobbyData = new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            { "startGame", new DataObject(DataObject.VisibilityOptions.Member, "true") },
                            { "relayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, RelayManager.Instance.joinCode) }
                        }
                    };
                    await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, lobbyData);
                }

                Debug.Log("Game start request sent.");
                await GameManager.Instance.StartGameWithRelay(RelayManager.Instance.joinCode);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError($"Failed to start game: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Only the host can start the game.");
        }
    }
}
