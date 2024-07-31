using TMPro;
using UnityEngine;

public class UI_LobbyListItem : MonoBehaviour
{
    public TextMeshProUGUI lobbyNameText;
    public TextMeshProUGUI lobbyDescriptionText;
    public TextMeshProUGUI playerCountText;

    public void SetLobbyInfo(string lobbyName, string lobbyDescription, int currentPlayers, int maxPlayers)
    {
        lobbyNameText.text = lobbyName;
        lobbyDescriptionText.text = lobbyDescription;
        playerCountText.text = $"Players: {currentPlayers}/{maxPlayers}";
    }
}