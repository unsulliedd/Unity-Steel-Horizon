using UnityEngine;
using Unity.Netcode;

public class TitleScreenManager : MonoBehaviour
{

    public void StartNetworkAsHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartNetworkAsClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void ShutdownServer()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public void StartNewGame()
    {
        StartCoroutine(SaveGameManager.Instance.LoadGame());
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
