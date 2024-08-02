using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveGameManager : MonoBehaviour
{
    [SerializeField] private int worldSceneIndex = 2;
    public int loadingScreenIndex = 1;

    private SaveFileWriter saveFileWriter;

    [Header("Save/Load")]
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

    public AsyncOperation WorldSceneOperation { get; private set; }
    public PlayerClass[] playerClass;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
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

    public void NewGame(int selectedCharacterIndex)
    {
        InitializeSaveFileWriter();

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
                    characterName = "Player",
                    characterClass = playerClass[selectedCharacterIndex].className,
                    characterClassIndex = selectedCharacterIndex,
                    characterLevel = 1,
                    ownedWeapons = new List<Weapon>(),
                    currentHealth = playerClass[selectedCharacterIndex].health,
                    currentStamina = playerClass[selectedCharacterIndex].stamina,
                    vitality = playerClass[selectedCharacterIndex].baseVitality,
                    strength = playerClass[selectedCharacterIndex].baseStrength,
                };

                saveFileWriter.CreateNewSaveFile(currentCharacterData);
                emptySlotFound = true;
                break;
            }
        }
        Debug.Log("NewGame" + selectedCharacterIndex);

        if (!emptySlotFound)
            TitleScreenManager.Instance.NoEmptySlotPanelPopUp();
        else
            StartCoroutine(LoadWorldScene(selectedCharacterIndex));
    }

    public void SaveGame()
    {
        saveFileName = AssignFileNamebyCharacterSlot(currentCharacterSlot);
        InitializeSaveFileWriter();
        saveFileWriter.saveFileName = saveFileName;

        SaveGameCallbacks.SaveGame(ref currentCharacterData);

        saveFileWriter.CreateNewSaveFile(currentCharacterData);
    }

    public void LoadGame()
    {
        saveFileName = AssignFileNamebyCharacterSlot(currentCharacterSlot);
        InitializeSaveFileWriter();
        saveFileWriter.saveFileName = saveFileName;

        currentCharacterData = saveFileWriter.LoadSaveFile();

        StartCoroutine(LoadWorldScene(currentCharacterData.characterClassIndex));
    }

    public void LoadAllCharacterProfiles()
    {
        InitializeSaveFileWriter();
        for (int i = 0; i < characterSlots.Count; i++)
        {
            saveFileWriter.saveFileName = AssignFileNamebyCharacterSlot((CharacterSlot)i);
            characterSlots[i] = saveFileWriter.LoadSaveFile();
        }
    }

    public void DeleteSaveGame(CharacterSlot characterSlot)
    {
        InitializeSaveFileWriter();
        saveFileWriter.saveFileName = AssignFileNamebyCharacterSlot(characterSlot);
        saveFileWriter.DeleteSaveFile();
    }

    public bool IsSlotEmpty(CharacterSlot characterSlot)
    {
        InitializeSaveFileWriter();
        saveFileWriter.saveFileName = AssignFileNamebyCharacterSlot(characterSlot);
        return !saveFileWriter.CheckIfSaveFileExists();
    }

    public string AssignFileNamebyCharacterSlot(CharacterSlot characterSlot)
    {
        return $"SH_{(int)characterSlot + 1:00}";
    }

    private void InitializeSaveFileWriter()
    {
        if (saveFileWriter == null)
            saveFileWriter = new SaveFileWriter
            {
                saveFileDirectoryPath = Application.persistentDataPath + "/Saves/"
            };
    }

    public IEnumerator LoadWorldScene(int selectedCharacterIndex)
    {
        Debug.Log("Loading world scene...");
        AsyncOperation loadingSceneOperation = SceneManager.LoadSceneAsync(loadingScreenIndex, LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadingSceneOperation.isDone);
        Debug.Log("Loading scene loaded");
        Scene loadingScene = SceneManager.GetSceneByBuildIndex(loadingScreenIndex);
        SceneManager.SetActiveScene(loadingScene);
        Debug.Log("Loading scene activated");
        WorldSceneOperation = SceneManager.LoadSceneAsync(worldSceneIndex, LoadSceneMode.Single);
        yield return new WaitUntil(() => WorldSceneOperation.isDone);

        SaveGameCallbacks.LoadGame(ref currentCharacterData);
        GameManager.Instance.Spawn(selectedCharacterIndex);
    }

    public int GetWorldSceneIndex() => worldSceneIndex;
}
