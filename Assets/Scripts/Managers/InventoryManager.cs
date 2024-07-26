using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<Weapon> ownedWeapons = new List<Weapon>();
    private PlayerManager playerManager;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        if (playerManager.PlayerNetworkManager.IsOwner)
        {
            playerManager.PlayerNetworkManager.ownedWeaponData.OnListChanged += OnOwnedWeaponsChanged;

            AddInitialWeapon();
        }
    }

    private void AddInitialWeapon()
    {
        Weapon startingWeapon = playerManager.WeaponManager.weapons.Find(w => w.rarity == WeaponRarity.Uncommon);
        if (startingWeapon != null)
        {
            AddWeapon(startingWeapon);
        }
    }

    public void AddWeapon(Weapon newWeapon)
    {
        if (playerManager.PlayerNetworkManager.IsOwner && !HasWeapon(newWeapon))
        {
            playerManager.PlayerNetworkManager.AddWeaponServerRpc(new WeaponData(newWeapon.weaponID, newWeapon.rarity));
        }
    }

    public bool HasWeapon(Weapon weapon)
    {
        return playerManager.PlayerNetworkManager.ownedWeaponData.Contains(new WeaponData(weapon.weaponID, weapon.rarity));
    }

    public void OnWeaponAdded(string weaponID)
    {
        Weapon addedWeapon = FindWeaponByID(weaponID);
        if (addedWeapon != null && !ownedWeapons.Contains(addedWeapon))
        {
            ownedWeapons.Add(addedWeapon);
        }
    }

    private void OnOwnedWeaponsChanged(NetworkListEvent<WeaponData> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<WeaponData>.EventType.Add:
                OnWeaponAdded(changeEvent.Value.weaponID.ToString());
                break;
            case NetworkListEvent<WeaponData>.EventType.Remove:
                Weapon removedWeapon = FindWeaponByID(changeEvent.Value.weaponID.ToString());
                if (removedWeapon != null && ownedWeapons.Contains(removedWeapon))
                {
                    ownedWeapons.Remove(removedWeapon);
                }
                break;
        }
    }

    private Weapon FindWeaponByID(string weaponID)
    {
        return playerManager.WeaponManager.weapons.Find(w => w.weaponID == weaponID);
    }

    private void OnDestroy()
    {
        if (playerManager != null && playerManager.PlayerNetworkManager != null)
        {
            if (playerManager.PlayerNetworkManager.ownedWeaponData != null)
                playerManager.PlayerNetworkManager.ownedWeaponData.OnListChanged -= OnOwnedWeaponsChanged;
        }
    }
}