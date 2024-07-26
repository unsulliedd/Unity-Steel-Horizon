using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's weapons, including equipping, switching, and updating networked weapon transforms.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    public List<Weapon> weapons; // List of available weapons
    public Transform weaponHolder; // Transform to hold the equipped weapon
    private GameObject currentWeapon; // Currently equipped weapon GameObject
    private Transform currentMuzzle; // Transform of the current weapon's muzzle
    private ParticleSystem muzzleFlashParticle; // Particle system for the muzzle flash
    private PlayerNetworkManager playerNetworkManager; // Reference to the PlayerNetworkManager

    /// <summary>
    /// Initializes references.
    /// </summary>
    private void Awake()
    {
        playerNetworkManager = GetComponent<PlayerNetworkManager>();
    }

    /// <summary>
    /// Equips a weapon by index.
    /// </summary>
    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;

        if (currentWeapon != null)
            Destroy(currentWeapon); // Destroy the currently equipped weapon

        Weapon weaponToEquip = weapons[index];

        // Instantiate the new weapon
        currentWeapon = Instantiate(weaponToEquip.weaponPrefab, weaponHolder);
        // Set the weapon's position and rotation
        currentWeapon.transform.SetLocalPositionAndRotation(weaponToEquip.idlePosition, weaponToEquip.idleRotation);

        // Get the muzzle
        GetMuzzle(weaponToEquip);

        // Get the muzzle flash particle system
        GetMuzzleFlash(weaponToEquip);
    }

    private void GetMuzzleFlash(Weapon weaponToEquip)
    {
        if (currentMuzzle != null)
            muzzleFlashParticle = currentMuzzle.Find("Muzzle Flash").GetComponent<ParticleSystem>();
        else
            Debug.LogWarning("Muzzle not found in weapon prefab: " + weaponToEquip.weaponName);
    }

    private void GetMuzzle(Weapon weaponToEquip)
    {
        if (currentWeapon != null)
            currentMuzzle = currentWeapon.transform.Find("Muzzle");
        else
            Debug.LogWarning("Weapon prefab not found: " + weaponToEquip.weaponName);
    }

    /// <summary>
    /// Switches to the next weapon in the list.
    /// </summary>
    public void NextWeapon()
    {
        if (playerNetworkManager.IsOwner)
        {
            int newIndex = (playerNetworkManager.currentWeaponIndex.Value + 1) % weapons.Count;
            playerNetworkManager.EquipWeaponServerRpc(newIndex); // Request the server to equip the new weapon
        }
    }

    /// <summary>
    /// Switches to the previous weapon in the list.
    /// </summary>
    public void PreviousWeapon()
    {
        if (playerNetworkManager.IsOwner)
        {
            int newIndex = (playerNetworkManager.currentWeaponIndex.Value - 1 + weapons.Count) % weapons.Count;
            playerNetworkManager.EquipWeaponServerRpc(newIndex); // Request the server to equip the new weapon
        }
    }

    /// <summary>
    /// Gets the currently equipped weapon.
    /// </summary>
    public GameObject GetCurrentWeapon() => currentWeapon;

    /// <summary>
    /// Gets the muzzle transform of the currently equipped weapon.
    /// </summary>
    /// <returns>The muzzle transform of the currently equipped weapon.</returns>
    public Transform GetCurrentMuzzle() => currentMuzzle;

    /// <summary>
    /// Gets the muzzle flash particle system of the currently equipped weapon.
    /// </summary>
    public ParticleSystem GetMuzzleFlashParticle() => muzzleFlashParticle;

    /// <summary>
    /// Gets the index of the currently equipped weapon.
    /// </summary>
    public int GetCurrentWeaponIndex() => playerNetworkManager.currentWeaponIndex.Value;

    /// <summary>
    /// Updates the networked transform of the equipped weapon.
    /// </summary>
    public void UpdateWeaponNetworkTransform(Weapon weapon)
    {
        if (currentWeapon == null) return;

        if (playerNetworkManager.isAiming.Value)
            UpdateWeaponTransform(weapon.aimPosition, weapon.aimRotation);
        else
            UpdateWeaponTransform(weapon.idlePosition, weapon.idleRotation);

        // Set the weapon's position and rotation based on network values
        currentWeapon.transform.SetLocalPositionAndRotation(playerNetworkManager.weaponPosition.Value, playerNetworkManager.weaponRotation.Value);
    }

    /// <summary>
    /// Updates the weapon transform based on the network values.
    /// </summary>
    private void UpdateWeaponTransform(Vector3 position, Quaternion rotation)
    {
        if (playerNetworkManager.IsOwner)
        {
            playerNetworkManager.weaponPosition.Value = position;
            playerNetworkManager.weaponRotation.Value = rotation;
        }
        else
        {
            position = playerNetworkManager.weaponPosition.Value;
            rotation = playerNetworkManager.weaponRotation.Value;
        }
    }
}