using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SoloCharacterSelection : MonoBehaviour
{
    public GameObject characterSelectionPanel;
    public Button[] characterButtons;
    public TextMeshProUGUI[] playerNamesText;
    public Button confirmButton;

    private int selectedCharacterIndex = -1;

    private void Start()
    {
        foreach (var button in characterButtons)
        {
            button.onClick.AddListener(() => OnCharacterButtonClicked(button));
        }
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private void OnCharacterButtonClicked(Button button)
    {
        selectedCharacterIndex = System.Array.IndexOf(characterButtons, button);
        Debug.Log($"Character {selectedCharacterIndex} selected");

        if (selectedCharacterIndex != -1)
        {
            playerNamesText[selectedCharacterIndex].text = "Selected";
        }
        else
            playerNamesText[selectedCharacterIndex].text = " ";
    }

    private void OnConfirmButtonClicked()
    {
        if (selectedCharacterIndex != -1)
        {
            Debug.Log($"Character {selectedCharacterIndex} confirmed");

            SaveGameManager.Instance.singlePlayer = true;
            SaveGameManager.Instance.NewGame(selectedCharacterIndex);
        }
    }

    public void ShowCharacterSelectionPanel()
    {
        characterSelectionPanel.SetActive(true);
    }
}
