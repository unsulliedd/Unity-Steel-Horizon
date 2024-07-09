using System.Collections;
using UnityEngine;

public class PlayerLocomotionManager : CharacterLocomotionManager
{
    public float horizontalMoveInput;
    public float verticalMoveInput;
    public float moveAmount;

    private Vector3 moveDirection;
    private Vector3 targetRotation;
    [SerializeField] private float runSpeed = 10;
    [SerializeField] private float walkSpeed = 5;
    [SerializeField] private float rotationSpeed = 10;

    [Header("Roll Settings")]
    [SerializeField] private Vector3 rollDirection;
    [SerializeField] private float sprintingRollSpeed = 20f;

    private PlayerManager playerManager;

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
            playerManager.CharacterNetworkManager.networkVertical.Value = verticalMoveInput;
            playerManager.CharacterNetworkManager.networkHorizontal.Value = horizontalMoveInput;
            playerManager.CharacterNetworkManager.networkMoveAmount.Value = moveAmount;
        }
        else
        {
            verticalMoveInput = playerManager.CharacterNetworkManager.networkVertical.Value;
            horizontalMoveInput = playerManager.CharacterNetworkManager.networkHorizontal.Value;
            moveAmount = playerManager.CharacterNetworkManager.networkMoveAmount.Value;

            playerManager.PlayerAnimationManager.MovementAnimations(horizontalMoveInput, verticalMoveInput);
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
        GetMovementInput();

        moveDirection = PlayerCamera.Instance.transform.forward * verticalMoveInput;
        moveDirection += PlayerCamera.Instance.transform.right * horizontalMoveInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (PlayerInputManager.Instance.moveAmount > 0.5f)
        {
            // Running Speed
            playerManager.CharacterController.Move(runSpeed * Time.deltaTime * moveDirection);
        }
        else if (PlayerInputManager.Instance.moveAmount <= 0.5f)
        {
            // Walking Speed
            playerManager.CharacterController.Move(walkSpeed * Time.deltaTime * moveDirection);
        }
    }

    private void HandleRotation()
    {
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

            if (moveAmount > 0.5f)
            {
                StartCoroutine(PerformSprintingRoll());
                playerManager.PlayerAnimationManager.PlayTargetAnimation("Sprinting Forward Roll", true, false, false, false);
            }
            else
                playerManager.PlayerAnimationManager.PlayTargetAnimation("Stand To Roll", true, true, false, false);
        }
        else
            playerManager.PlayerAnimationManager.PlayTargetAnimation("Standing Dodge Backward", true, true, false, false);
    }

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
}
