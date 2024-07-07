using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterManager : NetworkBehaviour
{
    public CharacterController CharacterController { get; private set; }
    public Animator Animator { get; private set; }
    public CharacterNetworkManager CharacterNetworkManager { get; private set; }
    protected virtual void Awake()
    {
        // This will make sure that the character manager is not destroyed when loading a new scene
        DontDestroyOnLoad(gameObject);

        Animator = GetComponent<Animator>();
        CharacterController = GetComponent<CharacterController>();
        CharacterNetworkManager = GetComponent<CharacterNetworkManager>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        // If the player is the owner of the character, then update the network position
        if (IsOwner)
        {
            CharacterNetworkManager.networkPosition.Value = transform.position; // Update the network position
            CharacterNetworkManager.networkRotation.Value = transform.rotation; // Update the network rotation
        }
        // If the player is not the owner of the character, then update the position of the character
        else
        {
            // Smoothly move the character to the network position
            // Smoothly rotate the character to the network rotation
            transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position,
                CharacterNetworkManager.networkPosition.Value,
                ref CharacterNetworkManager.networkSmoothVelocity,
                CharacterNetworkManager.networkVelocitySmoothTime),
                Quaternion.Slerp(transform.rotation,
                CharacterNetworkManager.networkRotation.Value,
                CharacterNetworkManager.networkRotationSmoothTime));
        }
    }

    protected virtual void LateUpdate()
    {

    }
}