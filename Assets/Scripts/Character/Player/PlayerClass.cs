using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct WeaponPosition
{
    public string weaponName; // Silah adý veya ID
    public Vector3 idlePosition;
    public Quaternion idleRotation;
    public Vector3 aimPosition;
    public Quaternion aimRotation;
    public Vector3 scale;
}

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Stats")]
public class PlayerClass : ScriptableObject
{
    public string className;
    public string classDescription;

    public int health;
    public int stamina;

    public float healthRegenAmount;
    public float staminaRegenAmount;

    public int baseDamage;
    public int baseArmor;
    public float critChance;
    public float critDamage;

    public float baseSprintingStaminaCost;
    public float baseRollStaminaCost;
    public float baseBackstepStaminaCost;
    public float baseJumpStaminaCost;

    public int baseStrength;
    public int baseVitality;
    public int baseLuck;

    public string[] abilities;
    public float abilityCooldown;
    public float abilityEffect;

    public Sprite classIcon;
    public Sprite classInGameImage;

    [Header("Weapon Holding Positions")]
    public List<WeaponPosition> weaponPositions;

    private Dictionary<string, WeaponPosition> weaponPositionDictionary;

    private void OnEnable()
    {
        weaponPositionDictionary = new Dictionary<string, WeaponPosition>();
        foreach (var weaponPosition in weaponPositions)
        {
            weaponPositionDictionary[weaponPosition.weaponName] = weaponPosition;
        }
    }

    public bool TryGetWeaponPosition(string weaponName, out WeaponPosition weaponPosition)
    {
        return weaponPositionDictionary.TryGetValue(weaponName, out weaponPosition);
    }
}
