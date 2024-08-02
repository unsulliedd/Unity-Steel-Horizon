using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterManager : NetworkBehaviour
{
    public Animator Animator { get; private set; }
    public CharacterController CharacterController { get; private set; }
    public CharacterNetworkManager CharacterNetworkManager { get; private set; }
    public CharacterAnimationManager CharacterAnimationManager { get; private set; }
    public CharacterEffectsManager CharacterEffectsManager { get; private set; }
    
    public WeaponManager WeaponManager { get; private set; }
    public AudioSource audioSource;
    public AudioSource environmentAudioSource;

    [Header("Flags")]
    public bool isPerformingAction = false;
    public bool isGrounded = true;
    public bool applyRootMotion = false;
    public bool canMove = true;
    public bool canRotate = true;

    public bool IsDead => CharacterNetworkManager.isDead.Value;

    protected virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        Animator = GetComponent<Animator>();
        CharacterController = GetComponent<CharacterController>();
        CharacterNetworkManager = GetComponent<CharacterNetworkManager>();
        CharacterAnimationManager = GetComponent<CharacterAnimationManager>();
        CharacterEffectsManager = GetComponent<CharacterEffectsManager>();
        WeaponManager = GetComponent<WeaponManager>();
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if (IsDead) return;

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

    public virtual IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation=false)
    {
        if (IsOwner)
        {
            CharacterNetworkManager.health.Value = 0;
            CharacterNetworkManager.isDead.Value = true;
        }
        if (!manuallySelectDeathAnimation)
        {
           CharacterAnimationManager.PlayTargetAnimation("Death", true);
        }

        yield return new WaitForSeconds(5);
    }

    public virtual void ReviveCharacter()
    {
        
    }
}