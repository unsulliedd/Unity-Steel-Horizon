using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject[] characterPrefabs; // Karakter prefablarý

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

    public async Task StartGameWithRelay(string relayJoinCode)
    {
        Debug.Log(relayJoinCode);
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("NetworkManager is already running.");
            return;
        }

        if (LobbyManager.Instance.GetHostId() == AuthenticationService.Instance.PlayerId)
        {
            Debug.Log("Starting as host");
            RelayManager.Instance.joinCode = relayJoinCode;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                RelayManager.Instance.allocation.RelayServer.IpV4,
                (ushort)RelayManager.Instance.allocation.RelayServer.Port,
                RelayManager.Instance.allocation.AllocationIdBytes,
                RelayManager.Instance.allocation.Key,
                RelayManager.Instance.allocation.ConnectionData
            );
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.Log("Starting as client");
            var joinAllocation = await RelayManager.Instance.JoinRelay(relayJoinCode);
            if (joinAllocation != null)
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );
                NetworkManager.Singleton.StartClient();
            }
        }

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

    public void StartSinglePlayerGame()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
    }

    private void SpawnPlayerCharacter(ulong clientId, int selectedCharacterIndex)
    {
        // Get the selected character index from the SaveGameManager
        int characterIndex = SaveGameManager.Instance.currentCharacterData.characterClassIndex;

        if (characterIndex < 0 || characterIndex >= characterPrefabs.Length)
        {
            Debug.LogError("Invalid character index");
            return;
        }

        GameObject characterPrefab = characterPrefabs[selectedCharacterIndex];
        GameObject characterInstance = Instantiate(characterPrefab);

        var networkObject = characterInstance.GetComponent<NetworkObject>();

        networkObject.SpawnAsPlayerObject(clientId);
        Debug.Log($"Spawned player character for client {clientId}");
        // Set the character data on the PlayerManager component
        PlayerManager playerManager = characterInstance.GetComponent<PlayerManager>();
        playerManager.SetCharacterData(SaveGameManager.Instance.currentCharacterData);
    }
}