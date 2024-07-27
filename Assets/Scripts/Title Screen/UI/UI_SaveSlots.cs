using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Manages the display and functionality of save slots in the UI.
/// </summary>
public class UI_SaveSlots : MonoBehaviour
{
    [Header("Character Slot")]
    [SerializeField] private CharacterSlot characterSlot;       // The character slot this UI element represents

    [Header("Save Slots Character Infos")]
    [SerializeField] private TextMeshProUGUI characterName;     // UI text for character name
    [SerializeField] private TextMeshProUGUI characterLevel;    // UI text for character level
    [SerializeField] private TextMeshProUGUI totalPlayTime;     // UI text for total playtime
    [SerializeField] private TextMeshProUGUI slotNumber;        // UI text for slot number

    private SaveFileWriter saveFileWriter;              // Handles the writing and reading of save files

    private void OnEnable()
    {
        LoadSaveSlot();
    }

    /// <summary>
    /// Loads the save slot information and updates the UI accordingly.
    /// </summary>
    private void LoadSaveSlot()
    {
        InitializeSaveFileWriter();

        // Get the save file name for this slot
        saveFileWriter.saveFileName = SaveGameManager.Instance.AssignFileNamebyCharacterSlot(characterSlot);

        if (saveFileWriter.CheckIfSaveFileExists())
        {
            // Load the character data from the save file
            CharacterSaveData characterData = saveFileWriter.LoadSaveFile();
            UpdateSaveSlotUI(characterData);
        }
        else
            ClearSaveSlotUI();

        slotNumber.text = $"Slot {(int)characterSlot + 1}";
    }

    /// <summary>
    /// Loads the game from the selected save slot.
    /// </summary>
    public void LoadGameFromSelectedSlot()
    {
        // Set the current character slot in the save game manager
        SaveGameManager.Instance.currentCharacterSlot = characterSlot;

        // Trigger the game load process
        SaveGameManager.Instance.LoadGame();
    }

    /// <summary>
    /// Selects the current slot.
    /// </summary>
    public void SelectCurrentSlot()
    {
        // Trigger the slot selection process
        TitleScreenManager.Instance.SelectCharacterSlot(characterSlot);
    }

    /// <summary>
    /// Initializes the save file writer.
    /// </summary>
    private void InitializeSaveFileWriter()
    {
        if (saveFileWriter == null)
            saveFileWriter = new SaveFileWriter
            {
                saveFileDirectoryPath = Application.persistentDataPath + "/Saves/"
            };
    }

    /// <summary>
    /// Updates the save slot UI with the given character data.
    /// </summary>
    /// <param name="characterData">The data of the character to display</param>
    private void UpdateSaveSlotUI(CharacterSaveData characterData)
    {
        characterName.text = characterData.characterName;
        characterLevel.text = $"Level: {characterData.characterLevel}";

        // Calculate playtime from total playtime in seconds
        TimeSpan playTime = TimeSpan.FromSeconds(characterData.totalPlayTime);

        // Calculate total hours including days
        int totalHours = (int)playTime.TotalHours;

        // Format the playtime as "HH:MM:SS"
        totalPlayTime.text = $"{totalHours:D2}:{playTime.Minutes:D2}:{playTime.Seconds:D2}";
    }

    /// <summary>
    /// Clears the save slot UI, indicating an empty slot.
    /// </summary>
    private void ClearSaveSlotUI()
    {
        characterName.text = "Empty Slot";
        characterLevel.text = "";
        totalPlayTime.text = "";
    }
}
