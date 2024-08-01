using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }
    private Lobby currentLobby;
    private ILobbyEvents currentLobbyEvents;
    public string hostId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (LobbyService.Instance != null)
        {
            // Register event handlers
            if (currentLobby != null)
            {
                await SubscribeToLobbyEvents();
            }
        }
    }

    private void OnDestroy()
    {
        if (currentLobbyEvents != null)
        {
            currentLobbyEvents.Callbacks.LobbyChanged -= OnLobbyChanged;
            currentLobbyEvents.Callbacks.KickedFromLobby -= OnKickedFromLobby;
            currentLobbyEvents.Callbacks.LobbyEventConnectionStateChanged -= OnLobbyEventConnectionStateChanged;
        }
    }

    private async Task SubscribeToLobbyEvents()
    {
        if (currentLobby != null)
        {
            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            callbacks.KickedFromLobby += OnKickedFromLobby;
            callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

            try
            {
                currentLobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(currentLobby.Id, callbacks);
                Debug.Log("Subscribed to lobby events.");
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError($"Failed to subscribe to lobby events: {ex.Message}");
            }
        }
    }

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            currentLobby = null;
            Debug.Log("Lobby has been deleted.");
            UI_Lobby.Instance.ClearLobbyDetails();
            UI_Lobby.Instance.listLobbiesButton.onClick.Invoke();
        }
        else
        {
            changes.ApplyToLobby(currentLobby);
            Debug.Log("Lobby changes applied.");
            UI_Lobby.Instance.UpdateLobbyDetails(currentLobby);
            UI_Lobby.Instance.listLobbiesButton.onClick.Invoke();

            // Check if the game should start
            if (currentLobby.Data.ContainsKey("startGame") && currentLobby.Data["startGame"].Value == "true")
            {
                Debug.Log("StartGame flag is set to true in the lobby.");
                UI_CharacterSelection.Instance.ShowCharacterSelection();
                UI_CharacterSelection.Instance.StartCountdown();
            }
        }
    }

    private void OnKickedFromLobby()
    {
        currentLobbyEvents = null;
        currentLobby = null;
        Debug.Log("Kicked from the lobby.");
        UI_Lobby.Instance.ClearLobbyDetails();
    }

    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        switch (state)
        {
            case LobbyEventConnectionState.Unsubscribed:
                Debug.Log("Unsubscribed from lobby events.");
                break;
            case LobbyEventConnectionState.Subscribing:
                Debug.Log("Subscribing to lobby events...");
                break;
            case LobbyEventConnectionState.Subscribed:
                Debug.Log("Subscribed to lobby events.");
                break;
            case LobbyEventConnectionState.Unsynced:
                Debug.Log("Connection problems with lobby events. Reconnecting...");
                break;
            case LobbyEventConnectionState.Error:
                Debug.LogError("Error with lobby event connection.");
                break;
        }
    }

    public async Task<Lobby> CreateLobby(string lobbyName, string lobbyDescription, int maxPlayers)
    {
        string joinCode = await RelayManager.Instance.CreateRelay();
        if (joinCode == null)
        {
            Debug.LogError("Failed to create relay.");
            return null;
        }

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = false,
            Data = new Dictionary<string, DataObject>
            {
                { "description", new DataObject(DataObject.VisibilityOptions.Public, lobbyDescription) },
                { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
            }
        };

        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            currentLobby = lobby;
            Debug.Log(lobby.HostId);
            hostId = AuthenticationService.Instance.PlayerId;
            Debug.Log(hostId);
            Debug.Log($"Lobby created with ID: {lobby.Id}");
            await SubscribeToLobbyEvents();

            return lobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to create lobby: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Lobby>> ListLobbies()
    {
        QueryLobbiesOptions options = new QueryLobbiesOptions();
        QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
        return response.Results;
    }

    public async Task JoinLobby(string lobbyId)
    {
        if (currentLobby != null && currentLobby.Id == lobbyId)
        {
            Debug.LogError("Player is already a member of this lobby.");
            return;
        }

        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            currentLobby = lobby;
            Debug.Log($"Joined lobby with ID: {lobby.Id}");

            await SubscribeToLobbyEvents();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to join lobby: {ex.Message}");
        }
    }

    public async Task LeaveLobby()
    {
        if (currentLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
                currentLobby = null;
                Debug.Log("Left the lobby.");

                if (currentLobbyEvents != null)
                {
                    await currentLobbyEvents.UnsubscribeAsync();
                    currentLobbyEvents = null;
                }
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError($"Failed to leave lobby: {ex.Message}");
            }
        }
    }

    public Lobby GetCurrentLobby()
    {
        return currentLobby;
    }

    public string GetHostId()
    {
        return hostId;
    }

    public async Task StartGameInLobby()
    {
        if (currentLobby != null)
        {
            var lobbyData = new Dictionary<string, DataObject>
            {
                { "startGame", new DataObject(DataObject.VisibilityOptions.Member, "true") }
            };

            try
            {
                await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions { Data = lobbyData });
                Debug.Log("Lobby data updated to start the game.");
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError($"Failed to update lobby: {ex.Message}");
            }
        }
    }

    public async void SubscribeLobbyEventsOnLobbyMenu()
    {
        await SubscribeToLobbyEvents();
        await ListLobbies();
    }
}
