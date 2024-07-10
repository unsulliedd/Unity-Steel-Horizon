using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : CharacterManager
{
    [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
    [HideInInspector] public PlayerAnimationManager PlayerAnimationManager;
    [HideInInspector] public PlayerNetworkManager PlayerNetworkManager;

    protected override void Awake()
    {
        base.Awake();

        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        PlayerAnimationManager = GetComponent<PlayerAnimationManager>();
        PlayerNetworkManager = GetComponent<PlayerNetworkManager>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        // Prevent the player controlling other players
        if (!IsOwner) return;

        playerLocomotionManager.HandleAllMovement();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (!IsOwner) return;

        PlayerCamera.Instance.HandleAllCameraAction();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            PlayerCamera.Instance.playerManager = this;
            PlayerInputManager.Instance.playerManager = this;
        }
    }
}
