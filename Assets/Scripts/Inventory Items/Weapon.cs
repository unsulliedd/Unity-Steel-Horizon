using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public enum WeaponRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponID;
    public string weaponBaseName;
    public string weaponName;
    public GameObject weaponPrefab;
    public float fireRate;
    public int damage;
    public int ammoCapacity;
    public float reloadTime;
    public WeaponRarity rarity;

    public Vector3 idlePosition;
    public Quaternion idleRotation;
    public Vector3 aimPosition;
    public Quaternion aimRotation;

    public Transform muzzleTransform;

    public Sprite weaponIcon;

    [Header("Upgrade Costs")]
    [SerializeField]
    private List<UpgradeCost> upgradeCosts;

    public MaterialCost GetUpgradeCost(WeaponRarity currentRarity)
    {
        foreach (var upgradeCost in upgradeCosts)
        {
            if (upgradeCost.fromRarity == currentRarity)
                return upgradeCost.cost;
        }
        return default;
    }
}

[Serializable]
public struct MaterialCost
{
    public int chips;
    public int cables;
    public int gears;
    public int pipes;

    public MaterialCost(int chips, int cables, int gears, int pipes)
    {
        this.chips = chips;
        this.cables = cables;
        this.gears = gears;
        this.pipes = pipes;
    }
}

[Serializable]
public struct UpgradeCost
{
    public WeaponRarity fromRarity;
    public WeaponRarity toRarity;
    public MaterialCost cost;

    public UpgradeCost(WeaponRarity fromRarity, WeaponRarity toRarity, MaterialCost cost)
    {
        this.fromRarity = fromRarity;
        this.toRarity = toRarity;
        this.cost = cost;
    }
}

public struct WeaponData : INetworkSerializable, IEquatable<WeaponData>
{
    public FixedString64Bytes weaponID;
    public WeaponRarity rarity;

    public WeaponData(string id, WeaponRarity rarity)
    {
        this.weaponID = new FixedString64Bytes(id);
        this.rarity = rarity;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref weaponID);
        serializer.SerializeValue(ref rarity);
    }

    public bool Equals(WeaponData other)
    {
        return weaponID.Equals(other.weaponID) && rarity == other.rarity;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + weaponID.GetHashCode();
            hash = hash * 23 + rarity.GetHashCode();
            return hash;
        }
    }
}