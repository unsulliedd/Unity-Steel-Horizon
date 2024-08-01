//using UnityEngine;
//using Unity.Netcode;
//using Unity.Networking.Transport;

//public class CustomNetworkManager : NetworkManager
//{
//    public GameObject[] characterPrefabs;

//    public override void OnServerAddPlayer(NetworkConnection conn, )
//    {
//        // Get the selected character index from the SaveGameManager
//        int characterIndex = SaveGameManager.Instance.currentCharacterData.characterClassIndex;

//        if (characterIndex < 0 || characterIndex >= characterPrefabs.Length)
//        {
//            Debug.LogError("Invalid character index");
//            return;
//        }

//        GameObject playerPrefab = characterPrefabs[characterIndex];
//        // Instantiate the selected character prefab
//        GameObject playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

//        // Spawn the player object on the network
//        NetworkServer.AddPlayerForConnection(conn, playerInstance);
//    }
//}
