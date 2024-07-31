using UnityEngine;
using TMPro;

public class UI_LobbyPlayerListItem : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;

    public void SetPlayerInfo(string playerId, string playerName)
    {
        playerNameText.text = $"{playerName} ({playerId})";
    }
}
