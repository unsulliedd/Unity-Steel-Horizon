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

    private Dictionary<ulong, int> clientCharacterSelections = new Dictionary<ulong, int>();

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
            SaveGameManager.Instance.StartCoroutine(SaveGameManager.Instance.LoadWorldScene(SaveGameManager.Instance.currentCharacterData.characterClassIndex));
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

    public void SpawnPlayers()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                int characterIndex = clientCharacterSelections.ContainsKey(clientId) ? clientCharacterSelections[clientId] : 0;
                SpawnPlayerCharacter(clientId, characterIndex);
            }
        }
    }

    private void SpawnPlayerCharacter(ulong clientId, int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characterPrefabs.Length)
        {
            Debug.LogError("Invalid character index");
            return;
        }

        GameObject characterPrefab = characterPrefabs[characterIndex];
        GameObject characterInstance = Instantiate(characterPrefab);

        var networkObject = characterInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);
        Debug.Log($"Spawned player character for client {clientId}");
    }

    public void SetCharacterIndexForClient(ulong clientId, int characterIndex)
    {
        if (clientCharacterSelections.ContainsKey(clientId))
        {
            clientCharacterSelections[clientId] = characterIndex;
        }
        else
        {
            clientCharacterSelections.Add(clientId, characterIndex);
        }
    }
}
