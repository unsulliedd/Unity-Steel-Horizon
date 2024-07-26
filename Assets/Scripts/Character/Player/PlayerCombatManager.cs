using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Manages player combat, including aiming and shooting mechanics.
/// </summary>
public class PlayerCombatManager : CharacterCombatManager
{
    #region References
    private PlayerManager playerManager;    // Reference to the player manager
    private PlayerCamera playerCamera;      // Reference to the player camera
    #endregion

    [Header("Combat")]
    [SerializeField] private bool isInCombatMode;           // Flag to indicate if the player is in combat mode
    [SerializeField] private bool isAiming;                 // Flag to indicate if the player is aiming

    [Header("Shooting")]
    private float nextFireTime = 0f;                        // Time until the player can fire the next shot
    [SerializeField] private float aimRange = 100f;         // Maximum range for aiming

    [Header("Aiming")]
    private Vector3 aimPoint;                               // The point the player is aiming at
    [SerializeField] private LayerMask aimLayerMask;        // Layer mask for aiming raycast

    [Header("Animation Rigging")]
    [SerializeField] private Rig aimRifleRig;               // Rig for aiming the rifle
    [SerializeField] private Transform aimConstraintSource; // Source transform for the aim constraint

    /// <summary>
    /// Initializes references and equips the initial weapon.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        playerManager = GetComponent<PlayerManager>();
        playerCamera = PlayerCamera.Instance;

        // Equip the initial weapon at the start
        EquipInitialWeapon(); 
    }

    /// <summary>
    /// Handles updating combat-related states and actions.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        GetCombatInputs();                  // Get inputs for combat actions
        UpdateNetworkCombatModeState();     // Update combat mode state over the network
        UpdateNetworkAnimationRigging();    // Update animation rigging over the network
        EnableCurrentWeapon();              // Enable the currently equipped weapon
        UpdateCombatAnimations();           // Update combat animations
        UpdateNetworkAimPoint();            // Update aim point over the network
        HandleAiming();                     // Handle aiming actions
    }

    /// <summary>
    /// Gets inputs for combat actions from the player.
    /// </summary>
    private void GetCombatInputs()
    {
        isInCombatMode = PlayerInputManager.Instance.combatMode;
        isAiming = PlayerInputManager.Instance.aimInput;
    }

    /// <summary>
    /// Updates the player's combat animations based on the current state.
    /// </summary>
    private void UpdateCombatAnimations()
    {
        playerManager.Animator.SetBool("IsInCombatMode", playerManager.PlayerNetworkManager.isInCombatMode.Value);
        playerManager.Animator.SetBool("IsAiming", playerManager.PlayerNetworkManager.isAiming.Value);
    }

    /// <summary>
    /// Updates the networked combat mode state.
    /// </summary>
    private void UpdateNetworkCombatModeState()
    {
        if (playerManager.IsOwner)
        {
            playerManager.PlayerNetworkManager.isInCombatMode.Value = isInCombatMode;
            if (playerManager.PlayerNetworkManager.isInCombatMode.Value)
                playerManager.PlayerNetworkManager.isAiming.Value = isAiming;
        }
        else
        {
            isInCombatMode = playerManager.PlayerNetworkManager.isInCombatMode.Value;
            isAiming = playerManager.PlayerNetworkManager.isAiming.Value;
        }
    }

    /// <summary>
    /// Updates the animation rigging weights and aim point over the network.
    /// </summary>
    private void UpdateNetworkAnimationRigging()
    {
        if (playerManager.IsOwner)
        {
            playerManager.PlayerNetworkManager.aimRifleRigWeight.Value = aimRifleRig.weight;
            playerManager.PlayerNetworkManager.aimPoint.Value = aimConstraintSource.position;
        }
        else
        {
            aimRifleRig.weight = playerManager.PlayerNetworkManager.aimRifleRigWeight.Value;
            aimConstraintSource.position = playerManager.PlayerNetworkManager.aimPoint.Value;
        }
    }

    /// <summary>
    /// Enables the currently equipped weapon if the player is in combat mode.
    /// </summary>
    private void EnableCurrentWeapon()
    {
        GameObject currentWeapon = playerManager.WeaponManager.GetCurrentWeapon();
        if (currentWeapon != null)
        {
            currentWeapon.SetActive(playerManager.PlayerNetworkManager.isInCombatMode.Value);
            playerManager.WeaponManager.UpdateWeaponNetworkTransform(
                playerManager.WeaponManager.weapons[playerManager.PlayerNetworkManager.currentWeaponIndex.Value]);
        }
    }

    /// <summary>
    /// Handles the aiming mechanics for the player.
    /// </summary>
    public void HandleAiming()
    {
        if (!playerManager.IsOwner) return;

        if (playerManager.PlayerNetworkManager.isAiming.Value)
        {
            playerCamera.crosshair.enabled = true;
            aimRifleRig.weight = 1;
            aimConstraintSource.position = aimPoint;
            playerCamera.Enable3rdPersonAimCamera();
            PerformRaycastAiming(); // Perform raycast for aiming
        }
        else
        {
            playerCamera.crosshair.enabled = false;
            aimRifleRig.weight = 0;
            playerCamera.Enable3rdPersonCamera();
        }

        UpdateNetworkCrosshairVisibility(); // Update the crosshair visibility over the network
        playerManager.WeaponManager.UpdateWeaponNetworkTransform(
            playerManager.WeaponManager.weapons[playerManager.PlayerNetworkManager.currentWeaponIndex.Value]);
    }

    /// <summary>
    /// Performs a raycast to determine the aim point.
    /// </summary>
    private void PerformRaycastAiming()
    {
        Ray ray = playerCamera.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, aimRange, aimLayerMask))
            aimPoint = hit.point; // Set aim point to the hit point
        else
            aimPoint = ray.GetPoint(aimRange); // Set aim point to the maximum range point

        DrawAimPointGizmo(); // Draw gizmo for debugging
    }

    /// <summary>
    /// Draws a gizmo to visualize the aim point.
    /// </summary>
    private void DrawAimPointGizmo()
    {
        Transform currentMuzzle = playerManager.WeaponManager.GetCurrentMuzzle();
        if (currentMuzzle != null)
            Debug.DrawLine(currentMuzzle.position, aimPoint, Color.red); // Draw line from muzzle to aim point
    }

    /// <summary>
    /// Updates the aim point over the network.
    /// </summary>
    private void UpdateNetworkAimPoint()
    {
        if (playerManager.IsOwner)
            playerManager.PlayerNetworkManager.aimPoint.Value = aimPoint;
        else
            aimPoint = playerManager.PlayerNetworkManager.aimPoint.Value;
    }

    /// <summary>
    /// Updates the crosshair visibility over the network.
    /// </summary>
    private void UpdateNetworkCrosshairVisibility()
    {
        if (playerManager.IsOwner)
            playerManager.PlayerNetworkManager.isCrosshairVisible.Value = playerCamera.crosshair.enabled;
        else
            playerCamera.crosshair.enabled = playerManager.PlayerNetworkManager.isCrosshairVisible.Value;
    }

    /// <summary>
    /// Handles the shooting mechanics for the player.
    /// </summary>
    public void HandleShooting()
    {
        if (!playerManager.IsOwner || Time.time < nextFireTime) return;

        if (playerManager.PlayerNetworkManager.isAiming.Value)
        {
            nextFireTime = Time.time + playerManager.WeaponManager.weapons[playerManager.PlayerNetworkManager.currentWeaponIndex.Value].fireRate;

            Transform currentMuzzle = playerManager.WeaponManager.GetCurrentMuzzle();
            if (currentMuzzle != null)
            {
                Vector3 aimDirection = (aimPoint - currentMuzzle.position).normalized;
                Quaternion bulletRotation = Quaternion.LookRotation(aimDirection);
                playerManager.PlayerNetworkManager.ShootBulletServerRpc(currentMuzzle.position, bulletRotation); // Fire the bullet on the server
            }
        }
    }

    /// <summary>
    /// Equips the initial weapon for the player.
    /// </summary>
    private void EquipInitialWeapon()
    {
        playerManager.PlayerNetworkManager.EquipWeaponServerRpc(playerManager.PlayerNetworkManager.currentWeaponIndex.Value);
    }
}