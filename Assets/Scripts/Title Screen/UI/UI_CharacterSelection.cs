using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_CharacterSelection : MonoBehaviour
{
    public static UI_CharacterSelection Instance { get; private set; }

    public GameObject characterSelectionPanel;
    public Button[] characterButtons;
    public Button confirmButton;
    public TextMeshProUGUI timerText;

    private int selectedCharacterIndex = -1;
    private float countdown = 120f; // Timer s�resi saniye olarak
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
            countdown -= Time.deltaTime;
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

            // Se�ilen karakter indeksi ile yeni bir oyun ba�lat
            StartGame();
        }
    }

    public void ShowCharacterSelection()
    {
        characterSelectionPanel.SetActive(true);
        isCountdownActive = false;
        countdown = 10f; // Zamanlay�c�y� s�f�rla
    }

    public void StartCountdown()
    {
        isCountdownActive = true;
    }

    private void StartGame()
    {
        isCountdownActive = false;
        Debug.Log("Starting game...");
        SaveGameManager.Instance.NewGame(selectedCharacterIndex);
        // T�m oyuncular i�in oyunu ba�latma mant���n� ekle
    }
}
