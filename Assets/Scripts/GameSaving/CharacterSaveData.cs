using System.Collections;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData
{
    [Header("Character Info")]
    public string characterName;
    public string characterClass;
    public int characterLevel;

    public float totalPlayTime;

    [Header("World Coordinates")]
    public float positionX;
    public float positionY;
    public float positionZ;

    [Header("Character Stats")]
    public int totalDeaths;
    public int totalEnemiesKilled;
}
