using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject[] characterPrefabs;

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

    public async Task<string> StartHostWithRelay(int maxConnections = 5)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }

    public void StartSinglePlayerGame()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
        NetworkManager.Singleton.StartHost();
    }

    public void Spawn(int selectedCharacterIndex)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Debug.Log($"Spawning player for client {clientId}");
                SpawnPlayerCharacter(clientId, selectedCharacterIndex);
            }
        }
    }

    public void SpawnSinglePLayer(int selectedCharacterIndex)
    {
        StartSinglePlayerGame();
        GameObject characterPrefab = characterPrefabs[selectedCharacterIndex];
        GameObject characterInstance = Instantiate(characterPrefab);

        var networkObject = characterInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);

        PlayerManager playerManager = characterInstance.GetComponent<PlayerManager>();
        playerManager.SetCharacterData(SaveGameManager.Instance.currentCharacterData);
    }

    private void SpawnPlayerCharacter(ulong clientId, int selectedCharacterIndex)
    {
        if (selectedCharacterIndex < 0 || selectedCharacterIndex >= characterPrefabs.Length)
        {
            Debug.LogError("Invalid character index");
            return;
        }

        GameObject characterPrefab = characterPrefabs[selectedCharacterIndex];
        GameObject characterInstance = Instantiate(characterPrefab);

        var networkObject = characterInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);
        Debug.Log($"Spawned player character for client {clientId}");

        PlayerManager playerManager = characterInstance.GetComponent<PlayerManager>();
        playerManager.SetCharacterData(SaveGameManager.Instance.currentCharacterData);
    }
}
