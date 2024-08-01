using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;

public class UI_CharacterSelection : MonoBehaviour
{
    public static UI_CharacterSelection Instance { get; private set; }

    public GameObject characterSelectionPanel;
    public Button[] characterButtons;
    public Button confirmButton;
    public TextMeshProUGUI timerText;

    private int selectedCharacterIndex = -1;
    private float countdown = 120f; // Timer duration in seconds
    private bool isCountdownActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (var button in characterButtons)
        {
            button.onClick.AddListener(() => OnCharacterButtonClicked(button));
        }
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        characterSelectionPanel.SetActive(false);
    }

    private void Update()
    {
        if (isCountdownActive)
        {
            countdown -= Time.time;
            timerText.text = $"Game starts in: {Mathf.Ceil(countdown)}s";
            if (countdown <= 0)
            {
                StartGame();
            }
        }
    }

    private void OnCharacterButtonClicked(Button button)
    {
        selectedCharacterIndex = System.Array.IndexOf(characterButtons, button);
        Debug.Log($"Character {selectedCharacterIndex} selected");
    }

    private void OnConfirmButtonClicked()
    {
        if (selectedCharacterIndex != -1)
        {
            Debug.Log($"Character {selectedCharacterIndex} confirmed");
            characterSelectionPanel.SetActive(false);

            // Start a new game with the selected character index
            StartGame();
            // Optionally, you can start the game immediately or show a countdown
            // StartCountdown();
        }
    }

    public void ShowCharacterSelection()
    {
        characterSelectionPanel.SetActive(true);
        isCountdownActive = false;
        countdown = 10f; // Reset the timer
    }

    private void StartCountdown()
    {
        isCountdownActive = true;
    }

    private void StartGame()
    {
        isCountdownActive = false;
        Debug.Log("Starting game...");
        SaveGameManager.Instance.NewGame(selectedCharacterIndex);
        // Add logic to start the game for all players
    }
}