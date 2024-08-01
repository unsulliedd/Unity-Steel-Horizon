using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

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
            Destroy(gameObject);
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