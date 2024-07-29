using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/Stats")]
public class PlayerClass : ScriptableObject
{
    public string className;
    public string classDescription;

    public int health;
    public int stamina;

    public float healthRegenAmounth;
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
}