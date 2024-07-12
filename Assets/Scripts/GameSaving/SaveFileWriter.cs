using UnityEngine;
using System.IO;

public class SaveFileWriter
{
    public string saveFileName = "";
    public string saveFileDirectoryPath = "";


    public bool CheckIfSaveFileExists()
    {
        if (File.Exists(Path.Combine(saveFileDirectoryPath + saveFileName)))
            return true;
        else
            return false;
    }

    public void DeleteSaveFile()
    {
        if (CheckIfSaveFileExists())
            File.Delete(Path.Combine(saveFileDirectoryPath + saveFileName));
    }

    public void CreateNewSaveFile(CharacterSaveData characterSaveData)
    {
        string savePath = Path.Combine(saveFileDirectoryPath + saveFileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            Debug.Log("Directory created at: " + savePath);

            string data = JsonUtility.ToJson(characterSaveData);

            using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
            {
                using (StreamWriter fileWriter = new StreamWriter(fileStream))
                {
                    fileWriter.Write(data);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error creating save file: " + savePath + "\n" + e.Message);
        }
    }

    public CharacterSaveData LoadSaveFile()
    {
        CharacterSaveData characterSaveData = null;
        string loadPath = Path.Combine(saveFileDirectoryPath + saveFileName);

        if (File.Exists(loadPath))
        {
            try
            {
                string data = "";

                using (FileStream fileStream = new FileStream(loadPath, FileMode.Open))
                {
                    using (StreamReader fileReader = new StreamReader(fileStream))
                    {
                        data = fileReader.ReadToEnd();
                    }
                }

                characterSaveData = JsonUtility.FromJson<CharacterSaveData>(data);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading save file: " + loadPath + "\n" + e.Message);
            }
        }
        return characterSaveData;
    }
}