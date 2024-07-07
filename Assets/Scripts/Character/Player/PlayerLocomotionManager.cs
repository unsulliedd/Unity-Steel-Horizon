using UnityEngine;

public class PlayerLocomotionManager : CharacterLocomotionManager
{
    public float horizontalMoveInput;
    public float verticalMoveInput;
    public float moveAmount;

    private Vector3 moveDirection;
    private Vector3 targetRotaion;
    [SerializeField] private float runSpeed = 10;
    [SerializeField] private float walkSpeed = 5;
    [SerializeField] private float rotationSpeed = 3;

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
        CalculateMoveDirection();

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

    private void CalculateMoveDirection()
    {
        moveDirection = PlayerCamera.Instance.transform.forward * verticalMoveInput;
        moveDirection += PlayerCamera.Instance.transform.right * horizontalMoveInput;
        moveDirection.Normalize();
        moveDirection.y = 0;
    }

    private void HandleRotation()
    {
        targetRotaion = Vector3.zero;
        targetRotaion = PlayerCamera.Instance.playerCamera.transform.forward * verticalMoveInput;
        targetRotaion += PlayerCamera.Instance.playerCamera.transform.right * horizontalMoveInput;
        targetRotaion.Normalize();
        targetRotaion.y = 0;

        if (targetRotaion == Vector3.zero)
            targetRotaion = transform.forward;

        Quaternion newRotation = Quaternion.LookRotation(targetRotaion);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = targetRotation;
    }
}
