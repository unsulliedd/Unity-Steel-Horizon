using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_LobbyPlayerListItem : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;

    public void SetPlayerInfo(string playerId)
    {
        if (playerNameText == null)
        {
            Debug.LogError("UI element is not assigned in the inspector.");
            return;
        }

        playerNameText.text = $"Player ID: {playerId}";
    }
}
