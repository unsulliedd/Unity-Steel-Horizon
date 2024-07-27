using Unity.Netcode;
using UnityEngine;

public class CharacterNetworkManager : NetworkBehaviour
{
    #region References
    private CharacterManager characterManager;
    #endregion

    [Header("Network Position Variables")]
    public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Vector3 networkSmoothVelocity;
    public float networkVelocitySmoothTime = 0.1f;

    [Header("Network Rotation Variables")]
    public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public float networkRotationSmoothTime = 0.1f;

    [Header("Network Animation Variables")]
    public NetworkVariable<float> networkAnimationParamHorizontal = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> networkAnimationParamVertical = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> networkMoveAmount = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> networkInAirTimer = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> networkIsGrounded = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Flags")]
    public NetworkVariable<bool> isSprinting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isJumping = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("StatsStamina")]
    public NetworkVariable<float> stamina = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxStamina = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> strength = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [Header("StatsHealth")]
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> vitality = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    

    [Header("Combat")]
    public NetworkVariable<bool> isInCombatMode = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isAiming = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isCrosshairVisible = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> aimRifleRigWeight = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> aimPoint = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    public void CheckHPDeath(int oldValue,int newValue)
    {
        if (currentHealth.Value <= 0)
        {
            StartCoroutine(characterManager.ProcessDeathEvent());
        }

        if (characterManager.IsOwner)
        {
            if (currentHealth.Value > maxHealth.Value)
            {
                currentHealth.Value = maxHealth.Value;
            }
        }
    }

    public void UpdateNetworkAnimationParams(float vertical, float horizontal, float moveAmount, float inAirTimer, bool isGrounded)
    {
        networkAnimationParamVertical.Value = vertical;
        networkAnimationParamHorizontal.Value = horizontal;
        networkMoveAmount.Value = moveAmount;
        networkInAirTimer.Value = inAirTimer;
        networkIsGrounded.Value = isGrounded;
    }

    [Rpc(SendTo.Server)]
    public void NotifyServerOfActionAnimationsRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (IsServer)
            PlayActionAnimationsForAllClientsRpc(clientId, animationName, applyRootMotion);
    }

    [Rpc(SendTo.Everyone)]
    public void PlayActionAnimationsForAllClientsRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            PerformActionAnimationsFromServer(animationName, applyRootMotion);
    }

    private void PerformActionAnimationsFromServer(string animationId, bool applyRootMotion)
    {
        characterManager.Animator.applyRootMotion = applyRootMotion;
        characterManager.Animator.CrossFade(animationId, 0.2f);
    }

    [Rpc(SendTo.Server)]

    public void ShootBulletServerRpc(Vector3 position, Quaternion rotation)
    {
        ShootBulletClientRpc(position, rotation);
    }

    [Rpc(SendTo.Everyone)]
    public void ShootBulletClientRpc(Vector3 position, Quaternion rotation)
    {
        characterManager.WeaponManager.GetMuzzleFlashParticle().Play();

        GameObject bulletGameobject = ObjectPoolManager.Instance.SpawnFromPool("Bullet", position, rotation);
        Bullet bullet = bulletGameobject.GetComponent<Bullet>();
        bullet.rb.isKinematic = false;
        bullet.rb.velocity = bullet.transform.forward * bullet.bulletSpeed;
    }
}
