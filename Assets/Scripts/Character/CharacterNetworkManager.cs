using Unity.Netcode;
using UnityEngine;

public class CharacterNetworkManager : NetworkBehaviour
{
    #region    
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

    [Header("Flags")]
    public NetworkVariable<bool> isSprinting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Stats")]
    public NetworkVariable<float> stamina = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> maxStamina = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> strength = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    // Update network animation parameters
    public void UpdateNetworkAnimationParams(float vertical, float horizontal, float moveAmount)
    {
        networkAnimationParamVertical.Value = vertical;
        networkAnimationParamHorizontal.Value = horizontal;
        networkMoveAmount.Value = moveAmount;
    }

    // Notify the server of action animations
    [Rpc(SendTo.Server)]
    public void NotifyServerOfActionAnimationsRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (IsServer)
            PlayActionAnimationsForAllClientsRpc(clientId, animationName, applyRootMotion);
    }

    // Play action animations for all clients
    [Rpc(SendTo.Everyone)]
    public void PlayActionAnimationsForAllClientsRpc(ulong clientId, string animationName, bool applyRootMotion)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            PerformActionAnimationsFromServer(animationName, applyRootMotion);
    }

    // Perform action animations received from the server
    private void PerformActionAnimationsFromServer(string animationId, bool applyRootMotion)
    {
        characterManager.Animator.applyRootMotion = applyRootMotion;
        characterManager.Animator.CrossFade(animationId, 0.2f);
    }
}