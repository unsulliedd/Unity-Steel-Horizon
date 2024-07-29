using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's info menu UI, displaying weapon and player stats.
/// </summary>
public class PlayerInfoMenu_UI : MonoBehaviour
{
    [Header("Weapon Info")]
    [SerializeField] private Image weaponImage;
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private TextMeshProUGUI weaponRarityText;
    [SerializeField] private TextMeshProUGUI weaponDamageText;
    [SerializeField] private TextMeshProUGUI weaponFireRateText;
    [SerializeField] private TextMeshProUGUI weaponAmmoCapacityText;

    [Header("Player Stats")]
    [SerializeField] private Image playerImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI vitalityText;

    /// <summary>
    /// Updates the UI when the menu is enabled.
    /// </summary>
    private void OnEnable()
    {
        UpdateUI();
    }

    /// <summary>
    /// Updates the UI elements with the player's current weapon and stats.
    /// </summary>
    public void UpdateUI()
    {
        PlayerManager playerManager = PlayerUIManager.Instance.playerManager;
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager is null");
            return;
        }

        UpdateWeaponInfo(playerManager);
        UpdatePlayerStats(playerManager);
    }

    /// <summary>
    /// Updates the weapon information in the UI.
    /// </summary>
    private void UpdateWeaponInfo(PlayerManager playerManager)
    {
        Weapon currentWeapon = playerManager.WeaponManager.GetCurrentWeapon();
        if (currentWeapon != null)
        {
            weaponImage.sprite = currentWeapon.weaponIcon;
            weaponName.text = $"Name: {currentWeapon.weaponName}";
            weaponRarityText.text = $"Rarity: {currentWeapon.rarity}";
            weaponDamageText.text = $"Damage: {currentWeapon.damage}";
            weaponFireRateText.text = $"Fire Rate: {currentWeapon.fireRate}";
            weaponAmmoCapacityText.text = $"Ammo Capacity: {currentWeapon.ammoCapacity}";
        }
    }

    /// <summary>
    /// Updates the player's stats information in the UI.
    /// </summary>
    private void UpdatePlayerStats(PlayerManager playerManager)
    {
        PlayerClass stats = playerManager.playerClass;
        playerImage.sprite = stats.classInGameImage;
        healthText.text = $"Health: {stats.health}";
        staminaText.text = $"Stamina: {stats.stamina}";
        strengthText.text = $"Strength: {stats.baseStrength}";
        vitalityText.text = $"Vitality: {stats.baseVitality}";
    }
}