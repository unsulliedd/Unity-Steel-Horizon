using UnityEngine;
using System.IO;

/// <summary>
/// Handles saving and loading of character save data to and from files.
/// </summary>
public class SaveFileWriter
{
    public string saveFileName = "";            // The name of the save file
    public string saveFileDirectoryPath = "";   // The directory path where save files are stored

    /// <summary>
    /// Checks if the save file exists.
    /// </summary>
    public bool CheckIfSaveFileExists()
    {
        return File.Exists(Path.Combine(saveFileDirectoryPath, saveFileName));
    }

    /// <summary>
    /// Deletes the save file if it exists.
    /// </summary>
    public void DeleteSaveFile()
    {
        if (CheckIfSaveFileExists())
            File.Delete(Path.Combine(saveFileDirectoryPath, saveFileName));
    }

    /// <summary>
    /// Creates a new save file with the provided character save data.
    /// </summary>
    /// <param name="characterSaveData">The character save data to write to the file.</param>
    public void CreateNewSaveFile(CharacterSaveData characterSaveData)
    {
        // Construct the full path for the save file
        string savePath = Path.Combine(saveFileDirectoryPath, saveFileName); 

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)); // Ensure the directory exists
            Debug.Log("Directory created at: " + savePath);

            string data = JsonUtility.ToJson(characterSaveData); // Convert character save data to JSON

            // Write the JSON data to the save file
            using FileStream fileStream = new FileStream(savePath, FileMode.Create);
            using StreamWriter fileWriter = new StreamWriter(fileStream);
            fileWriter.Write(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error creating save file: " + savePath + "\n" + e.Message);
        }
    }

    /// <summary>
    /// Loads the save file and returns the character save data.
    /// </summary>
    /// <returns>The character save data loaded from the file, or null if the file does not exist or an error occurs.</returns>
    public CharacterSaveData LoadSaveFile()
    {
        CharacterSaveData characterSaveData = null;

        // Construct the full path for the save file
        string loadPath = Path.Combine(saveFileDirectoryPath, saveFileName); 

        if (File.Exists(loadPath))
        {
            try
            {
                string data = File.ReadAllText(loadPath); // Read the JSON data from the save file
                characterSaveData = JsonUtility.FromJson<CharacterSaveData>(data); // Convert the JSON data to CharacterSaveData
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading save file: " + loadPath + "\n" + e.Message);
            }
        }
        return characterSaveData;
    }
}