using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterManager : NetworkBehaviour
{
    public Animator Animator { get; private set; }
    public CharacterController CharacterController { get; private set; }
    public CharacterNetworkManager CharacterNetworkManager { get; private set; }
    public WeaponManager WeaponManager { get; private set; }

    [Header("Flags")]
    public bool isPerformingAction = false;
    public bool isGrounded = true;
    public bool applyRootMotion = false;
    public bool canMove = true;
    public bool canRotate = true;

    protected virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        Animator = GetComponent<Animator>();
        CharacterController = GetComponent<CharacterController>();
        CharacterNetworkManager = GetComponent<CharacterNetworkManager>();
        WeaponManager = GetComponent<WeaponManager>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        //If the player is the owner of the character, then update the network position
        if (IsOwner)
        {
            // Update the network position and rotation
            CharacterNetworkManager.networkPosition.Value = transform.position;
            CharacterNetworkManager.networkRotation.Value = transform.rotation;
        }
        else
        {
            // Smoothly move the character to the network position
            Vector3 smoothPosition = Vector3.SmoothDamp(transform.position,
                CharacterNetworkManager.networkPosition.Value,
                ref CharacterNetworkManager.networkSmoothVelocity,
                CharacterNetworkManager.networkVelocitySmoothTime);

            Quaternion smoothRotation = Quaternion.Slerp(transform.rotation,
                CharacterNetworkManager.networkRotation.Value,
                CharacterNetworkManager.networkRotationSmoothTime);

            transform.SetPositionAndRotation(smoothPosition, smoothRotation);
        }
    }
}