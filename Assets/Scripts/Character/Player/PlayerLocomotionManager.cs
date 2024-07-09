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
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Roll Settings")]
    [SerializeField] private Vector3 rollDirection;
    [SerializeField] private float sprintingRollSpeed = 20f;

    [Header("References")]
    private PlayerManager playerManager;

    private PlayerInputManager inputManager;
    private PlayerCamera playerCamera;

    protected override void Awake()
    {
        base.Awake();
        playerManager = GetComponent<PlayerManager>();
        inputManager = PlayerInputManager.Instance;
        playerCamera = PlayerCamera.Instance;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        SyncInputs();
    }

    private void SyncInputs()
    {
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
        }
    }

    public void HandleAllMovement()
    {
        HandleMovement();
        HandleRotation();
    }

    private void GetMovementInput()
    {
        horizontalMoveInput = inputManager.horizontalMoveInput;
        verticalMoveInput = inputManager.verticalMoveInput;
        moveAmount = inputManager.moveAmount;
    }

    private void HandleMovement()
    {
        GetMovementInput();

        if (!playerManager.canMove) return;

        moveDirection = playerCamera.transform.forward * verticalMoveInput +
                        playerCamera.transform.right * horizontalMoveInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        float speed = moveAmount > 0.5f ? runSpeed : walkSpeed;
        playerManager.CharacterController.Move(speed * Time.deltaTime * moveDirection);
    }

    private void HandleRotation()
    {
        if (!playerManager.canRotate) return;

        targetRotation = playerCamera.transform.forward * verticalMoveInput +
                         playerCamera.transform.right * horizontalMoveInput;
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
            rollDirection = playerCamera.transform.forward * verticalMoveInput +
                            playerCamera.transform.right * horizontalMoveInput;
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
