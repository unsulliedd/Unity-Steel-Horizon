using UnityEngine;
using Unity.Netcode;

public class CharacterNetworkManager : NetworkBehaviour
{
    CharacterManager characterManager;

    [Header("Network Position Variables")]
    public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Vector3 networkSmoothVelocity;
    public float networkVelocitySmoothTime = 0.1f;

    [Header("Network Rotation Variables")]
    public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public float networkRotationSmoothTime = 0.1f;

    [Header("Network Animation Variables")]
    public NetworkVariable<float> networkHorizontal = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> networkVertical = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> networkMoveAmount = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    [ServerRpc]
    public void NotifyServerOfActionAnimationsServerRpc(ulong clientId, string animatinName, bool applyRootMotion)
    {
        if (IsServer)
        {
            PlayActionAnimationsForAllClientsClientRpc(clientId, animatinName, applyRootMotion);
        }
    }

    [ClientRpc]
    public void PlayActionAnimationsForAllClientsClientRpc(ulong clientId, string animatinName, bool applyRootMotion)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            PerformActionAnimationsFromServer(animatinName, applyRootMotion);
        }
    }

    private void PerformActionAnimationsFromServer(string animationId, bool applyRootMotion)
    {
        characterManager.Animator.applyRootMotion = applyRootMotion;
        characterManager.Animator.CrossFade(animationId, 0.2f);
    }
}