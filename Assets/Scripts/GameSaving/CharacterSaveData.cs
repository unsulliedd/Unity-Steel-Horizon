using UnityEngine;

[System.Serializable]
public class CharacterSaveData
{
    [Header("Character Info")]
    public string characterName = "Character";
    public string characterClass;
    public int characterLevel = 1;

    public float totalPlayTime;

    [Header("World Coordinates")]
    public float positionX;
    public float positionY;
    public float positionZ;

    [Header("Character Stats")]
    public int totalDeaths;
    public int totalEnemiesKilled;

    [Header("PlayerStats")] 
    public int vitality;
    public int strenght;
    public float currentHealth;
    public float currentStamina;
}
