using System.Collections.Generic;
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

    [Header("Stats")] 
    public int currentHealth;
    public float currentStamina;
    public int vitality;
    public int strength;

    [Header("Inventory")]
    public List<Weapon> ownedWeapons;
    public int ownedJunkAmount;
    public int ownedAmmo;
    public int ownedChipAmount;
    public int ownedGearAmount;
    public int ownedCableAmount;
    public int ownedPipeAmount;

    [Header("Kill Records")]
    public int totalDeaths;
    public int totalEnemiesKilled;
    internal int characterClassIndex;
}