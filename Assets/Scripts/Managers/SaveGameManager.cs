using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages saving and loading game data, including character profiles and game state.
/// </summary>
public class SaveGameManager : MonoBehaviour
{
    [SerializeField] private int worldSceneIndex = 2; // Index of the world scene to load
    public int loadingScreenIndex = 1; // Index of the loading screen scene

    private SaveFileWriter saveFileWriter; // Handles the writing and reading of save files

    [Header("Save/Load")]
    public bool saveGame; // Flag to trigger game save
    public bool loadGame; // Flag to trigger game load

    [Header("Current Character Data")]
    public CharacterSlot currentCharacterSlot; // Current character slot being used
    public CharacterSaveData currentCharacterData; // Data of the current character
    private string saveFileName; // Name of the save file

    [Header("Character Slots")]
    public List<CharacterSaveData> characterSlots = new List<CharacterSaveData>(); // List of all character slots

    public PlayerManager playerManager; // Reference to the PlayerManager

    public static SaveGameManager Instance { get; private set; } // Singleton instance of SaveGameManager

    public AsyncOperation WorldSceneOperation { get; private set; } // AsyncOperation for world scene

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
        // Load all character profiles at the start
        LoadAllCharacterProfiles();
    }

    private void Update()
    {
        // Manual in-game save/load handling
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

    /// <summary>
    /// Starts a new game, finding an empty character slot and initializing it.
    /// </summary>
    public void NewGame()
    {
        InitializeSaveFileWriter();

        bool emptySlotFound = false;

        // Find an empty character slot
        for (int i = 0; i < characterSlots.Count; i++)
        {
            CharacterSlot slot = (CharacterSlot)i;
            saveFileName = AssignFileNamebyCharacterSlot(slot);

            saveFileWriter.saveFileName = saveFileName;

            if (!saveFileWriter.CheckIfSaveFileExists())
            {
                // Set the current character slot and default data
                currentCharacterSlot = slot;
                currentCharacterData = new CharacterSaveData
                {
                    characterName = "Character",
                    characterClass = null,
                    characterLevel = 1
                };

                // Create a new save file for the character
                saveFileWriter.CreateNewSaveFile(currentCharacterData);
                emptySlotFound = true;
                break;
            }
        }

        // Show no empty slot message
        if (!emptySlotFound)
            TitleScreenManager.Instance.NoEmptySlotPanelPopUp();
        else
            StartCoroutine(LoadWorldScene());
    }

    /// <summary>
    /// Saves the current game state to a file.
    /// </summary>
    public void SaveGame()
    {
        saveFileName = AssignFileNamebyCharacterSlot(currentCharacterSlot);

        InitializeSaveFileWriter();

        saveFileWriter.saveFileName = saveFileName;
        // Call save game callbacks
        SaveGameCallbacks.SaveGame(ref currentCharacterData);

        // Save the current character data
        saveFileWriter.CreateNewSaveFile(currentCharacterData);
    }

    /// <summary>
    /// Loads the game state from a file.
    /// </summary>
    public void LoadGame()
    {
        saveFileName = AssignFileNamebyCharacterSlot(currentCharacterSlot);

        InitializeSaveFileWriter();
        saveFileWriter.saveFileName = saveFileName;

        // Load the character data from the save file
        currentCharacterData = saveFileWriter.LoadSaveFile();

        StartCoroutine(LoadWorldScene());
    }

    /// <summary>
    /// Loads all character profiles from the save files.
    /// </summary>
    public void LoadAllCharacterProfiles()
    {
        InitializeSaveFileWriter();
        for (int i = 0; i < characterSlots.Count; i++)
        {
            // Load each character slot data
            saveFileWriter.saveFileName = AssignFileNamebyCharacterSlot((CharacterSlot)i);
            characterSlots[i] = saveFileWriter.LoadSaveFile();
        }
    }

    /// <summary>
    /// Deletes the save file for the specified character slot.
    /// </summary>
    public void DeleteSaveGame(CharacterSlot characterSlot)
    {
        InitializeSaveFileWriter(); // Ensure the save file writer is initialized
        saveFileWriter.saveFileName = AssignFileNamebyCharacterSlot(characterSlot);
        saveFileWriter.DeleteSaveFile(); // Delete the save file for the specified character slot
    }

    /// <summary>
    /// Checks if the specified character slot is empty.
    /// </summary>
    public bool IsSlotEmpty(CharacterSlot characterSlot)
    {
        InitializeSaveFileWriter();
        saveFileWriter.saveFileName = AssignFileNamebyCharacterSlot(characterSlot);

        // Check if the save file exists for the specified slot
        return !saveFileWriter.CheckIfSaveFileExists();
    }

    /// <summary>
    /// Assigns a file name based on the character slot.
    /// </summary>
    public string AssignFileNamebyCharacterSlot(CharacterSlot characterSlot)
    {
        // Return the appropriate file name based on the character slot
        return $"SH_{(int)characterSlot + 1:00}";
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
    /// Loads the world scene asynchronously.
    /// </summary>
    /// <returns>An enumerator for coroutine support.</returns>
    public IEnumerator LoadWorldScene()
    {
        AsyncOperation loadingSceneOperation = SceneManager.LoadSceneAsync(loadingScreenIndex, LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadingSceneOperation.isDone);

        Scene loadingScene = SceneManager.GetSceneByBuildIndex(loadingScreenIndex);
        SceneManager.SetActiveScene(loadingScene);

        WorldSceneOperation = SceneManager.LoadSceneAsync(worldSceneIndex, LoadSceneMode.Single);
        yield return new WaitUntil(() => WorldSceneOperation.isDone);


        if (TitleScreenManager.Instance.startAsHost)
            NetworkManager.Singleton.StartHost();
        else if (TitleScreenManager.Instance.startAsClient)
            NetworkManager.Singleton.StartClient();
        else
            NetworkManager.Singleton.StartHost();
        
        // Once the scene is loaded, load the game data
        SaveGameCallbacks.LoadGame(ref currentCharacterData);
    }

    public int GetWorldSceneIndex() => worldSceneIndex;
}
