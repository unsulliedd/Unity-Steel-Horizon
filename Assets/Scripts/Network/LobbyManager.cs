using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }
    private Lobby currentLobby;
    private ILobbyEvents currentLobbyEvents;

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
        if (LobbyService.Instance != null)
        {
            // Register event handlers
            await SubscribeToLobbyEvents();
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
            // Handle lobby being deleted
            currentLobby = null;
            Debug.Log("Lobby has been deleted.");
            // Update UI to reflect lobby deletion
            UI_Lobby.Instance.ClearLobbyDetails();
        }
        else
        {
            changes.ApplyToLobby(currentLobby);
            Debug.Log("Lobby changes applied.");
            // Refresh UI with updated lobby details
            UI_Lobby.Instance.UpdateLobbyDetails(currentLobby);
        }
    }

    private void OnKickedFromLobby()
    {
        currentLobbyEvents = null;
        currentLobby = null;
        Debug.Log("Kicked from the lobby.");
        // Update UI to reflect being kicked from the lobby
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
            Debug.Log($"Lobby created with ID: {lobby.Id}");

            // Subscribe to lobby events
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

            // Subscribe to lobby events
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
}
