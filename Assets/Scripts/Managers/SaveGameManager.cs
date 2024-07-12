using System.Collections;
using System.Collections.Generic;
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
    private string fileName;

    [Header("Character Slots")]
    public List<CharacterSaveData> characterSlots = new List<CharacterSaveData>();

    [SerializeField] private PlayerManager playerManager;


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
        AssignFileNamebyCharacterSlot();
        currentCharacterData = new CharacterSaveData();
        StartCoroutine(LoadWorldScene());
    }

    public void SaveGame()
    {
        AssignFileNamebyCharacterSlot();

        saveFileWriter = new SaveFileWriter();
        saveFileWriter.saveFileDirectoryPath = Application.persistentDataPath + "/Saves/";
        saveFileWriter.saveFileName = fileName;

        SaveGameEvent.SaveGame(currentCharacterData);

        saveFileWriter.CreateNewSaveFile(currentCharacterData);
    }

    public void LoadGame()
    {
        AssignFileNamebyCharacterSlot();

        saveFileWriter = new SaveFileWriter();
        saveFileWriter.saveFileDirectoryPath = Application.persistentDataPath + "/Saves/";
        saveFileWriter.saveFileName = fileName;

        currentCharacterData = saveFileWriter.LoadSaveFile();

        SaveGameEvent.LoadGame(currentCharacterData);

        StartCoroutine(LoadWorldScene());
    }

    private void AssignFileNamebyCharacterSlot()
    {
        switch (currentCharacterSlot)
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
    }

    public IEnumerator LoadWorldScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(worldSceneIndex);

        while (!asyncLoad.isDone)
            yield return null;
    }
}
