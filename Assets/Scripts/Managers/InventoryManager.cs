using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the player's inventory, including owned weapons, junk processing, and crafting.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    #region References
    private PlayerManager playerManager; // Reference to the PlayerManager
    #endregion

    public List<Weapon> ownedWeapons = new List<Weapon>(); // List of owned weapons

    [Header("Junk Processing")]
    public int junkAmount = 250; // Amount of junk in the inventory
    public int chips = 0; // Number of chips
    public int cables = 0; // Number of cables
    public int gears = 0; // Number of gears
    public int pipes = 0; // Number of pipes

    public event Action OnInventoryUpdated; // Event triggered when the inventory is updated


    #region Unity Methods
    /// <summary>
    /// Initializes references.
    /// </summary>
    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    /// <summary>
    /// Sets up the inventory and adds the initial weapon.
    /// </summary>
    private void Start()
    {
        if (playerManager.PlayerNetworkManager.IsOwner)
        {
            // Subscribe to the list changed event
            playerManager.PlayerNetworkManager.ownedWeaponData.OnListChanged += OnOwnedWeaponsChanged; 
            AddInitialWeapon(); // Add the initial weapon to the inventory
        }
    }

    /// <summary>
    /// Cleans up event subscriptions when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (playerManager != null && playerManager.PlayerNetworkManager != null)
            if (playerManager.PlayerNetworkManager.ownedWeaponData != null)
                playerManager.PlayerNetworkManager.ownedWeaponData.OnListChanged -= OnOwnedWeaponsChanged;
    }
    #endregion

    #region Weapon Management
    /// <summary>
    /// Adds the initial weapon to the inventory.
    /// </summary>
    private void AddInitialWeapon()
    {
        Weapon startingWeapon = playerManager.WeaponManager.weapons.Find(w => w.rarity == WeaponRarity.Common);
        if (startingWeapon != null)
            AddWeapon(startingWeapon);
    }

    /// <summary>
    /// Adds a new weapon to the inventory.
    /// </summary>
    public void AddWeapon(Weapon newWeapon)
    {
        if (playerManager.PlayerNetworkManager.IsOwner && !HasWeapon(newWeapon))
            playerManager.PlayerNetworkManager.AddWeaponServerRpc(new WeaponData(newWeapon.weaponID, newWeapon.rarity));
    }

    /// <summary>
    /// Removes a weapon from the inventory.
    /// </summary>
    public void RemoveWeapon(Weapon weapon)
    {
        if (playerManager.PlayerNetworkManager.IsOwner && HasWeapon(weapon))
            playerManager.PlayerNetworkManager.RemoveWeaponServerRpc(new WeaponData(weapon.weaponID, weapon.rarity));
    }

    /// <summary>
    /// Checks if the inventory already contains the specified weapon.
    /// </summary>
    public bool HasWeapon(Weapon weapon) => playerManager.PlayerNetworkManager.ownedWeaponData.Contains(new WeaponData(weapon.weaponID, weapon.rarity));

    /// <summary>
    /// Handles the event when a weapon is added.
    /// </summary>
    public void OnWeaponAdded(string weaponID)
    {
        Weapon addedWeapon = FindWeaponByID(weaponID);
        if (addedWeapon != null && !ownedWeapons.Contains(addedWeapon))
        {
            ownedWeapons.Add(addedWeapon);
            OnInventoryUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Handles the event when a weapon is removed.
    /// </summary>
    public void OnWeaponRemoved(string weaponID)
    {
        Weapon removedWeapon = FindWeaponByID(weaponID);
        if (removedWeapon != null && ownedWeapons.Contains(removedWeapon))
        {
            ownedWeapons.Remove(removedWeapon);
            OnInventoryUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Handles changes to the owned weapon data.
    /// </summary>
    private void OnOwnedWeaponsChanged(NetworkListEvent<WeaponData> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<WeaponData>.EventType.Add:
                OnWeaponAdded(changeEvent.Value.weaponID.ToString());
                break;
            case NetworkListEvent<WeaponData>.EventType.Remove:
                OnWeaponRemoved(changeEvent.Value.weaponID.ToString());
                break;
        }
    }

    /// <summary>
    /// Finds a weapon by its ID.
    /// </summary>
    private Weapon FindWeaponByID(string weaponID) => playerManager.WeaponManager.weapons.Find(w => w.weaponID == weaponID);
    #endregion

    #region Junk Processing
    /// <summary>
    /// Adds a specified amount of junk to the inventory.
    /// </summary>
    public void AddJunk(int amount)
    {
        junkAmount += amount;
        OnInventoryUpdated?.Invoke();
    }

    /// <summary>
    /// Processes junk into usable materials.
    /// </summary>
    public void ProcessJunk()
    {
        int totalJunk = junkAmount;
        int remainingJunk = totalJunk;

        chips = UnityEngine.Random.Range(0, remainingJunk + 1);
        remainingJunk -= chips;

        cables = UnityEngine.Random.Range(0, remainingJunk + 1);
        remainingJunk -= cables;

        gears = UnityEngine.Random.Range(0, remainingJunk + 1);
        remainingJunk -= gears;

        pipes = remainingJunk;

        junkAmount = 0;
        OnInventoryUpdated?.Invoke();
    }
    #endregion

    #region Crafting
    /// <summary>
    /// Updates the inventory for a crafted weapon.
    /// </summary>
    public void UpdateInventoryForCraftedWeapon(Weapon weapon, Weapon newWeapon)
    {
        if (newWeapon != null)
        {
            RemoveWeapon(weapon);
            AddWeapon(newWeapon);
            playerManager.WeaponManager.EquipCraftedWeapon(newWeapon.weaponID);
            OnInventoryUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Checks if the player has enough materials for upgrading the weapon.
    /// </summary>
    public bool HasMaterialsForUpgrade(Weapon weapon)
    {
        var upgradeCost = weapon.GetUpgradeCost(weapon.rarity);
        return chips >= upgradeCost.chips && cables >= upgradeCost.cables && gears >= upgradeCost.gears && pipes >= upgradeCost.pipes;
    }

    /// <summary>
    /// Consumes the necessary materials for upgrading the weapon.
    /// </summary>
    public void ConsumeMaterialsForUpgrade(Weapon weapon)
    {
        var upgradeCost = weapon.GetUpgradeCost(weapon.rarity);
        chips -= upgradeCost.chips;
        cables -= upgradeCost.cables;
        gears -= upgradeCost.gears;
        pipes -= upgradeCost.pipes;
    }

    /// <summary>
    /// Gets the next rarity level for the weapon.
    /// </summary>
    public WeaponRarity GetNextRarity(WeaponRarity currentRarity)
    {
        return currentRarity switch
        {
            WeaponRarity.Common => WeaponRarity.Uncommon,
            WeaponRarity.Uncommon => WeaponRarity.Rare,
            WeaponRarity.Rare => WeaponRarity.Epic,
            WeaponRarity.Epic => WeaponRarity.Legendary,
            _ => currentRarity,
        };
    }
    #endregion
}