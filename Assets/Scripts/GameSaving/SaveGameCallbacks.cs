public static class SaveGameCallbacks
{
    // Delegate definitions for save and load game callbacks with ref parameter
    public delegate void SaveGameDelegate(ref CharacterSaveData data);
    public delegate void LoadGameDelegate(ref CharacterSaveData data);

    // Static delegate instances to hold the registered callback methods
    public static SaveGameDelegate OnSaveGame;
    public static LoadGameDelegate OnLoadGame;

    // Method to invoke the OnSaveGame callback with a ref parameter
    public static void SaveGame(ref CharacterSaveData data)
    {
        OnSaveGame?.Invoke(ref data);
    }

    // Method to invoke the OnLoadGame callback with a ref parameter
    public static void LoadGame(ref CharacterSaveData data)
    {
        OnLoadGame?.Invoke(ref data);
    }
}