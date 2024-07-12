using System;

public class SaveGameEvent
{
    public static event Action<CharacterSaveData> OnSaveGame;
    public static event Action<CharacterSaveData> OnLoadGame;

    public static void SaveGame(CharacterSaveData data)
    {
        OnSaveGame?.Invoke(data);
    }

    public static void LoadGame(CharacterSaveData data)
    {
        OnLoadGame?.Invoke(data);
    }
}
