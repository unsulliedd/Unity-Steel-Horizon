using System.Collections;
using UnityEngine;

public class PlayerLocomotionManager : CharacterLocomotionManager
{
    public float horizontalMoveInput;
    public float verticalMoveInput;
    public float moveAmount;

    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 6;
    [SerializeField] private float walkSpeed = 3;
    [SerializeField] private float sprintSpeed = 8;
    [SerializeField] private float rotationSpeed = 10;
    private Vector3 moveDirection;
    private Vector3 targetRotation;

    [Header("Roll Settings")]
    [SerializeField] private float sprintingRollSpeed = 8;
    private Vector3 rollDirection;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2;
    [SerializeField] private float jumpDistance = 8;
    [SerializeField] private float inAirMoveSpeed = 8;
    private Vector3 inAirMovementDirection;

    [Header("Stamina Settings")]
    [SerializeField] private int sprintingStaminaCost = 10;
    [SerializeField] private int rollStaminaCost = 15;
    [SerializeField] private int backstepStaminaCost = 5;
    [SerializeField] private int jumpStaminaCost = 20;

    private PlayerManager playerManager;

    protected override void Awake()
    {
        base.Awake();
        playerManager = GetComponent<PlayerManager>();
    }

    protected override void Update()
    {
        base.Update();
        UpdateNetworkValues();
        HandleAllMovement();
    }

    // Update network values for animation parameters if the player is the owner, otherwise apply network values from other players
    private void UpdateNetworkValues()
    {
        if (playerManager.IsOwner)
            playerManager.CharacterNetworkManager.UpdateNetworkAnimationParams(verticalMoveInput, horizontalMoveInput, moveAmount, inAirTimer, playerManager.isGrounded);
        else
            ApplyNetworkValues();
    }

    // Apply network values received from other players
    private void ApplyNetworkValues()
    {
        verticalMoveInput = playerManager.CharacterNetworkManager.networkAnimationParamVertical.Value;
        horizontalMoveInput = playerManager.CharacterNetworkManager.networkAnimationParamHorizontal.Value;
        moveAmount = playerManager.CharacterNetworkManager.networkMoveAmount.Value;
        inAirTimer = playerManager.CharacterNetworkManager.networkInAirTimer.Value;
        playerManager.isGrounded = playerManager.CharacterNetworkManager.networkIsGrounded.Value;

        playerManager.PlayerAnimationManager.MovementAnimations(0, moveAmount, playerManager.PlayerNetworkManager.isSprinting.Value);
    }

    // Handle all movement operations
    public void HandleAllMovement()
    {
        if (!playerManager.IsOwner) return;
        HandleMovement();
        HandleRotation();
        HandleInAirMovement();
    }

    // Get movement input from the PlayerInputManager
    private void GetMovementInput()
    {
        horizontalMoveInput = PlayerInputManager.Instance.horizontalMoveInput;
        verticalMoveInput = PlayerInputManager.Instance.verticalMoveInput;
        moveAmount = PlayerInputManager.Instance.moveAmount;
    }

    // Handle player movement based on input
    private void HandleMovement()
    {
        if (!playerManager.canMove) return;

        GetMovementInput();

        moveDirection = PlayerCamera.Instance.mainCamera.transform.forward * verticalMoveInput +
                        PlayerCamera.Instance.mainCamera.transform.right * horizontalMoveInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        float speed = playerManager.PlayerNetworkManager.isSprinting.Value ? sprintSpeed : (moveAmount <= 0.5f ? walkSpeed : runSpeed);
        playerManager.CharacterController.Move(speed * Time.deltaTime * moveDirection);
    }

    // Handle player rotation based on input
    private void HandleRotation()
    {
        if (!playerManager.canRotate) return;
        if (playerManager.PlayerNetworkManager.isAiming.Value)
        {
            // While aiming, rotate the character to face the same direction as the camera
            Vector3 cameraForward = PlayerCamera.Instance.mainCamera.transform.forward;
            cameraForward.y = 0; // Keep the rotation horizontal
            if (cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Normal rotation based on movement direction
            targetRotation = Vector3.zero;
            targetRotation = PlayerCamera.Instance.mainCamera.transform.forward * verticalMoveInput +
                             PlayerCamera.Instance.mainCamera.transform.right * horizontalMoveInput;
            targetRotation.Normalize();
            targetRotation.y = 0;

            if (targetRotation == Vector3.zero)
                targetRotation = transform.forward;
            else
            {
                Quaternion newRotation = Quaternion.LookRotation(targetRotation);
                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleInAirMovement()
    {
        if (!playerManager.isGrounded)
        {
            inAirMovementDirection = PlayerCamera.Instance.mainCamera.transform.forward * verticalMoveInput +
                                     PlayerCamera.Instance.mainCamera.transform.right * horizontalMoveInput;

            inAirMovementDirection.Normalize();

            playerManager.CharacterController.Move(inAirMoveSpeed * Time.deltaTime * inAirMovementDirection);
        }
    }

    // Attempt to perform a roll action
    public void AttemptToPerformRoll()
    {
        if (playerManager.isPerformingAction) return;
        if (playerManager.PlayerNetworkManager.stamina.Value <= rollStaminaCost) return;

        if (moveAmount > 0)
        {
            rollDirection = PlayerCamera.Instance.mainCamera.transform.forward * verticalMoveInput +
                            PlayerCamera.Instance.mainCamera.transform.right * horizontalMoveInput;
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

            playerManager.PlayerNetworkManager.stamina.Value -= rollStaminaCost;
        }
        else
        {
            playerManager.PlayerAnimationManager.PlayTargetAnimation("Standing Dodge Backward", true, true, false, false);
            playerManager.PlayerNetworkManager.stamina.Value -= backstepStaminaCost;
        }
    }

    // Perform a sprinting roll over a duration of time
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

    // Handle sprinting input and state
    public void HandleSprinting()
    {
        if (playerManager.isPerformingAction)
            playerManager.PlayerNetworkManager.isSprinting.Value = false;
        else if (playerManager.PlayerNetworkManager.stamina.Value <= 0)
        {
            playerManager.PlayerNetworkManager.isSprinting.Value = false;
            return;
        }
        else
            playerManager.PlayerNetworkManager.isSprinting.Value = moveAmount >= 0.5f && PlayerInputManager.Instance.sprintInput;

        playerManager.PlayerNetworkManager.stamina.Value -= sprintingStaminaCost * Time.deltaTime;
    }

    // Attempt to perform a jump action
    public void AttemptToPerformJump()
    {
        if (playerManager.isPerformingAction) return;
        if (playerManager.PlayerNetworkManager.stamina.Value <= jumpStaminaCost) return;
        if (playerManager.PlayerNetworkManager.isJumping.Value) return;
        if (!playerManager.isGrounded) return;

        if (playerManager.PlayerNetworkManager.isSprinting.Value)
            playerManager.PlayerAnimationManager.PlayTargetAnimation("Big Jump", true, true);
        else if (moveAmount > 0)
            playerManager.PlayerAnimationManager.PlayTargetAnimation("Running Jump", true, true);
        else
            playerManager.PlayerAnimationManager.PlayTargetAnimation("Jump", true, true);

        playerManager.CharacterNetworkManager.isJumping.Value = true;
        playerManager.PlayerNetworkManager.stamina.Value -= jumpStaminaCost;
    }

    // Handle the jump action
    public void ApplyJumpVelocity()
    {
        inAirVelocity = moveDirection * jumpDistance;
        // Calculate vertical velocity for the jump
        inAirVelocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(gravity));
    }
}
