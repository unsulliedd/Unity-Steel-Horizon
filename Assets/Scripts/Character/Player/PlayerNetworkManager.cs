using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages network-related functionalities for the player, including weapon management and synchronization.
/// </summary>
public class PlayerNetworkManager : CharacterNetworkManager
{
    #region References
    private PlayerManager playerManager; // Reference to the PlayerManager
    #endregion

    // Network variable to store the character's name
    public NetworkVariable<FixedString64Bytes> characterName = new NetworkVariable<FixedString64Bytes>("Character", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Network list to store data about owned weapons
    public NetworkList<WeaponData> ownedWeaponData;

    // Network variables to manage the current weapon's index, position, and rotation
    public NetworkVariable<int> currentWeaponIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> weaponPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> weaponRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    /// <summary>
    /// Initializes references and network lists.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        playerManager = GetComponent<PlayerManager>();

        // Initialize the network list for owned weapons
        ownedWeaponData = new NetworkList<WeaponData>(); 
    }

    /// <summary>
    /// Server RPC to equip a weapon at the specified index.
    /// </summary>
    [Rpc(SendTo.Server)]
    public void EquipWeaponServerRpc(int index)
    {
        currentWeaponIndex.Value = index;   // Update the current weapon index on the server
        EquipWeaponClientRpc(index);        // Notify clients to equip the weapon
    }

    /// <summary>
    /// Client RPC to equip a weapon at the specified index.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void EquipWeaponClientRpc(int index)
    {
        // Equip the weapon locally on clients
        playerManager.WeaponManager.EquipWeapon(index); 
    }

    /// <summary>
    /// Server RPC to add a weapon to the owned weapon data.
    /// </summary>
    [Rpc(SendTo.Server)]
    public void AddWeaponServerRpc(WeaponData weaponData)
    {
        ownedWeaponData.Add(weaponData); // Add the weapon data to the network list on the server
        AddWeaponClientRpc(weaponData);  // Notify clients to add the weapon to their inventory
    }

    /// <summary>
    /// Client RPC to add a weapon to the inventory.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    private void AddWeaponClientRpc(WeaponData weaponData)
    {
        // Add the weapon to the local inventory on clients
        playerManager.InventoryManager.OnWeaponAdded(weaponData.weaponID.ToString()); 
    }

    /// <summary>
    /// Cleans up network collections when the object is destroyed.
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}