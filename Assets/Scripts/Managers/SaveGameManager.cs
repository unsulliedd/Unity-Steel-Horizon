using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveGameManager : MonoBehaviour
{
    [SerializeField] private int worldSceneIndex = 1;

    private SaveFileWriter saveFileWriter;

    [Header("Save/Load)")]
    public bool saveGame;
    public bool loadGame;

    [Header("Current Character Data")]
    public CharacterSlot currentCharacterSlot;
    public CharacterSaveData currentCharacterData;
    private string saveFileName;

    [Header("Character Slots")]
    public List<CharacterSaveData> characterSlots = new List<CharacterSaveData>();

    public PlayerManager playerManager;

    public static SaveGameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        LoadAllCharacterProfiles();
    }

    private void Update()
    {
        if (saveGame)
        {
            SaveGame();
            saveGame = false;
        }

        if (loadGame)
        {
            LoadGame();
            loadGame = false;
        }
    }

    public void NewGame()
    {
        saveFileWriter = new SaveFileWriter();
        saveFileWriter.saveFileDirectoryPath = Application.persistentDataPath + "/Saves/";

        bool emptySlotFound = false;

        for (int i = 0; i < characterSlots.Count; i++)
        {
            CharacterSlot slot = (CharacterSlot)i;
            saveFileName = AssignFileNamebyCharacterSlot(slot);

            saveFileWriter.saveFileName = saveFileName;

            if (!saveFileWriter.CheckIfSaveFileExists())
            {
                currentCharacterSlot = slot;
                currentCharacterData = new CharacterSaveData
                {
                    characterName = "Character",
                    characterClass = null,
                    characterLevel = 1
                };

                saveFileWriter.CreateNewSaveFile(currentCharacterData);
                emptySlotFound = true;
                break;
            }
        }

        if (!emptySlotFound)
        {
            Debug.LogError("No empty save slots available.");
            return;
        }

        StartCoroutine(LoadWorldScene());
    }


    public void SaveGame()
    {
        saveFileName = AssignFileNamebyCharacterSlot(currentCharacterSlot);

        saveFileWriter = new SaveFileWriter();
        saveFileWriter.saveFileDirectoryPath = Application.persistentDataPath + "/Saves/";
        saveFileWriter.saveFileName = saveFileName;

        SaveGameCallbacks.SaveGame(ref currentCharacterData);

        saveFileWriter.CreateNewSaveFile(currentCharacterData);
    }

    public void LoadGame()
    {
        saveFileName = AssignFileNamebyCharacterSlot(currentCharacterSlot);

        saveFileWriter = new SaveFileWriter();
        saveFileWriter.saveFileDirectoryPath = Application.persistentDataPath + "/Saves/";
        saveFileWriter.saveFileName = saveFileName;

        currentCharacterData = saveFileWriter.LoadSaveFile();

        StartCoroutine(LoadWorldScene());
    }

    public void LoadAllCharacterProfiles()
    {
        saveFileWriter = new SaveFileWriter();
        saveFileWriter.saveFileDirectoryPath = Application.persistentDataPath + "/Saves/";
        for (int i = 0; i < characterSlots.Count; i++)
        {
            saveFileWriter.saveFileName = AssignFileNamebyCharacterSlot((CharacterSlot)i);
            characterSlots[i] = saveFileWriter.LoadSaveFile();
        }
    }

    public string AssignFileNamebyCharacterSlot(CharacterSlot characterSlot)
    {
        string fileName = "";

        switch (characterSlot)
        {
            case CharacterSlot.Slot01:
                fileName = "SH_01";
                break;
            case CharacterSlot.Slot02:
                fileName = "SH_02";
                break;
            case CharacterSlot.Slot03:
                fileName = "SH_03";
                break;
            case CharacterSlot.Slot04:
                fileName = "SH_04";
                break;
            case CharacterSlot.Slot05:
                fileName = "SH_05";
                break;
            case CharacterSlot.Slot06:
                fileName = "SH_06";
                break;
            case CharacterSlot.Slot07:
                fileName = "SH_07";
                break;
            case CharacterSlot.Slot08:
                fileName = "SH_08";
                break;
            case CharacterSlot.Slot09:
                fileName = "SH_09";
                break;
            case CharacterSlot.Slot10:
                fileName = "SH_10";
                break;
            default:
                break;
        }

        return fileName;
    }

    public IEnumerator LoadWorldScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(worldSceneIndex);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Once the scene is loaded, load the game data
        SaveGameCallbacks.LoadGame(ref currentCharacterData);
    }
}
