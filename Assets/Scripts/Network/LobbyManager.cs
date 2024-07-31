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
