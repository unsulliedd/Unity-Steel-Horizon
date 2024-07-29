using UnityEngine;

/// <summary>
/// Manages crafting and upgrading weapons for the player.
/// </summary>
public class CraftingManager : MonoBehaviour
{
    private PlayerManager playerManager;
    private Weapon selectedWeapon;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    /// <summary>
    /// Selects a weapon for crafting or upgrading.
    /// </summary>
    public void SelectWeaponForCrafting(Weapon weapon) => selectedWeapon = weapon;

    /// <summary>
    /// Attempts to craft or upgrade the selected weapon.
    /// </summary>
    public void TryCraftWeaponUpgrade()
    {
        if (selectedWeapon != null && playerManager.InventoryManager.HasMaterialsForUpgrade(selectedWeapon))
            CraftWeapon(selectedWeapon);
        else
            ShowNotEnoughMaterialsMessage();
    }

    /// <summary>
    /// Crafts or upgrades the specified weapon.
    /// </summary>
    private void CraftWeapon(Weapon weapon)
    {
        playerManager.InventoryManager.ConsumeMaterialsForUpgrade(weapon);
        WeaponRarity nextRarity = playerManager.InventoryManager.GetNextRarity(weapon.rarity);
        Weapon newWeapon = playerManager.WeaponManager.weapons.Find(w => w.rarity == nextRarity);
        playerManager.InventoryManager.UpdateInventoryForCraftedWeapon(weapon, newWeapon);
    }

    /// <summary>
    /// Decomposes junk items into useful materials.
    /// </summary>
    public void DecomposeJunk() => playerManager.InventoryManager.ProcessJunk();

    /// <summary>
    /// Shows a message indicating that there are not enough materials to craft or upgrade the weapon.
    /// </summary>
    private void ShowNotEnoughMaterialsMessage()
    {
        PlayerUIManager.Instance.playerInventoryUI.ShowNotEnoughMaterialsMessage();
        PlayerUIManager.Instance.playerInventoryUI.craftWeaponButton.interactable = false;
    }
}