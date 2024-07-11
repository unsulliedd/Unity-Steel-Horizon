using System.Collections;
using UnityEngine;

public class PlayerLocomotionManager : CharacterLocomotionManager
{
    public float horizontalMoveInput;
    public float verticalMoveInput;
    public float moveAmount;

    [Header("Movement Settings")]
    private Vector3 moveDirection;
    private Vector3 targetRotation;
    [SerializeField] private float runSpeed = 8;
    [SerializeField] private float walkSpeed = 4;
    [SerializeField] private float sprintSpeed = 10;
    [SerializeField] private float rotationSpeed = 10;

    [Header("Roll Settings")]
    private Vector3 rollDirection;
    [SerializeField] private float sprintingRollSpeed = 10;


    #region References
    private PlayerManager playerManager;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        playerManager = GetComponent<PlayerManager>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (playerManager.IsOwner)
        {
            // If the player is the owner, update the network values with the local values
            playerManager.CharacterNetworkManager.networkAnimationParamVertical.Value = verticalMoveInput;
            playerManager.CharacterNetworkManager.networkAnimationParamHorizontal.Value = horizontalMoveInput;
            playerManager.CharacterNetworkManager.networkMoveAmount.Value = moveAmount;
        }
        else
        {
            // If the player is not the owner, update the client's player animation
            verticalMoveInput = playerManager.CharacterNetworkManager.networkAnimationParamVertical.Value;
            horizontalMoveInput = playerManager.CharacterNetworkManager.networkAnimationParamHorizontal.Value;
            moveAmount = playerManager.CharacterNetworkManager.networkMoveAmount.Value;

            // Update the client's player animation
            playerManager.PlayerAnimationManager.MovementAnimations(0, moveAmount, playerManager.PlayerNetworkManager.isSprinting.Value);
        }
    }

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void GetMovementInput()
    {
        horizontalMoveInput = PlayerInputManager.Instance.horizontalMoveInput;
        verticalMoveInput = PlayerInputManager.Instance.verticalMoveInput;
        moveAmount = PlayerInputManager.Instance.moveAmount;
    }

    private void HandleMovement()
    {
        if (!playerManager.canMove) return;

        GetMovementInput();

        moveDirection = PlayerCamera.Instance.transform.forward * verticalMoveInput;
        moveDirection += PlayerCamera.Instance.transform.right * horizontalMoveInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (playerManager.PlayerNetworkManager.isSprinting.Value)
        {
            playerManager.CharacterController.Move(sprintSpeed * Time.deltaTime * moveDirection);

        }
        else
        {
            float speed = PlayerInputManager.Instance.moveAmount <= 0.5f ? walkSpeed : runSpeed;
            playerManager.CharacterController.Move(speed * Time.deltaTime * moveDirection);
        }

    }

    private void HandleRotation()
    {
        if (!playerManager.canRotate) return;

        targetRotation = Vector3.zero;
        targetRotation = PlayerCamera.Instance.playerCamera.transform.forward * verticalMoveInput;
        targetRotation += PlayerCamera.Instance.playerCamera.transform.right * horizontalMoveInput;
        targetRotation.Normalize();
        targetRotation.y = 0;

        if (targetRotation == Vector3.zero)
            targetRotation = transform.forward;

        Quaternion newRotation = Quaternion.LookRotation(targetRotation);
        Quaternion newTargetRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = newTargetRotation;
    }

    // Attempt to perform a roll action
    public void AttemptToPerformRoll()
    {
        if (playerManager.isPerformingAction) return;

        if (moveAmount > 0)
        {
            rollDirection = PlayerCamera.Instance.playerCamera.transform.forward * verticalMoveInput +
                            PlayerCamera.Instance.playerCamera.transform.right * horizontalMoveInput;
            rollDirection.Normalize();
            rollDirection.y = 0;

            Quaternion playerRotation = Quaternion.LookRotation(rollDirection);
            transform.rotation = playerRotation;

            if (moveAmount > 0.5f && playerManager.PlayerNetworkManager.isSprinting.Value) 
            { 
                StartCoroutine(PerformSprintingRoll());
                playerManager.PlayerAnimationManager.PlayTargetAnimation("Sprinting Forward Roll", true, true, false, false);
            }
            else
                playerManager.PlayerAnimationManager.PlayTargetAnimation("Stand To Roll", true, true, false, false);
        }
        else
            playerManager.PlayerAnimationManager.PlayTargetAnimation("Standing Dodge Backward", true, true, false, false);
    }

    // Sprint Roll Coroutine
    private IEnumerator PerformSprintingRoll()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            playerManager.CharacterController.Move(sprintingRollSpeed * Time.deltaTime * rollDirection);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void HandleSprinting()
    { 
        if (playerManager.isPerformingAction)
            playerManager.PlayerNetworkManager.isSprinting.Value = false;

        if (moveAmount >= 0.5f)
            playerManager.PlayerNetworkManager.isSprinting.Value = PlayerInputManager.Instance.sprintInput;
        else
            playerManager.PlayerNetworkManager.isSprinting.Value = false;
    }
}
