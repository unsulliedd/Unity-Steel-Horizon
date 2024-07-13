using UnityEngine;
using TMPro;
using System;

public class UI_SaveSlots : MonoBehaviour
{
    [Header("Save File Writer")]
    SaveFileWriter saveFileWriter;

    [Header("Character Slot")]
    public CharacterSlot characterSlot;

    [Header("Save Slots Character Infos")]
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI characterLevel;
    [SerializeField] private TextMeshProUGUI totalPlayTime;
    [SerializeField] private TextMeshProUGUI slotNumber;

    private void OnEnable()
    {
        LoadSaveSlot();
    }

    private void LoadSaveSlot()
    {
        saveFileWriter = new SaveFileWriter();
        saveFileWriter.saveFileDirectoryPath = Application.persistentDataPath + "/Saves/";

        saveFileWriter.saveFileName = SaveGameManager.Instance.AssignFileNamebyCharacterSlot(characterSlot);

        if (saveFileWriter.CheckIfSaveFileExists())
        {
            CharacterSaveData characterData = saveFileWriter.LoadSaveFile();

            characterName.text = characterData.characterName;
            characterLevel.text = "Level: " + characterData.characterLevel.ToString();
            TimeSpan playTime = TimeSpan.FromSeconds(characterData.totalPlayTime);
            totalPlayTime.text = playTime.ToString(@"hh\:mm\:ss");
        }
        else
        {
            characterName.text = "Empty Slot";
            characterLevel.text = "";
            totalPlayTime.text = "";
        }

        slotNumber.text = "Slot " + ((int)characterSlot + 1);
    }

    public void LoadGameFromSelectedSlot()
    {
        SaveGameManager.Instance.currentCharacterSlot = characterSlot;
        TitleScreenManager.Instance.LoadGame();
    }

    public void SelectCurrentSlot()
    {
        TitleScreenManager.Instance.SelectCharacterSlot(characterSlot);
    }
}
