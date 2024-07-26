using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public class PlayerInputManager : MonoBehaviour
{
    #region Singleton
    public static PlayerInputManager Instance { get; private set; }
    #endregion

    #region Components
    public PlayerControls playerControls;
    [HideInInspector] public PlayerManager playerManager;
    #endregion

    [Header("Move Input")]
    [SerializeField] private Vector2 moveInput;
    public float horizontalMoveInput;
    public float verticalMoveInput;
    public float moveAmount;

    [Header("Action Input")]
    public bool rollInput;
    public bool sprintInput;
    public bool jumpInput;

    [Header("Look Input")]
    [SerializeField] private Vector2 lookInput;
    public float horizontalLookInput;
    public float verticalLookInput;
    
    [Header("Drive Input")]
    [SerializeField] private Vector2 driveInput;
    public float horizontalDriveInput;
    public float verticalDriveInput;
    public bool enterVehicleInput;
    public bool brakeVehicleInput;

    // UI Actions
    [Header("UI Input")]
    public Vector2 navigationInput;
    public Vector2 scrollWheelInput;
    public bool submitPerformed;
    public bool deletePerformed;
    public bool cancelPerformed;
    public bool clickPerformed;
    public bool rightClickPerformed;
    public bool anyKeyPerformed;


    [Header("Controller Type")]
    public bool isGamepadActive = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerControls = new PlayerControls();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneChanged;
        InputSystem.onEvent += OnInputEvent;

        playerControls.PlayerMovement.Disable();
        playerControls.PlayerActions.Disable();
        playerControls.PlayerCamera.Disable();
        playerControls.VehicleControls.Disable();
        playerControls.UI.Enable();
        playerControls.Interactions.Disable();

        AssignMovementInputs();
        AssignActionInputs();
        AssignCameraInput();
        AssignUIInputs();
        AssignDriveInputs();
        AssignInteractionInputs();
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        InputSystem.onEvent -= OnInputEvent;
    }

    void Update()
    {
        HandleMoveInput();
        HandleLookInput();
        HandleDodgeInput();
        HandleSprintInput();
        HandleJumpInput();
        HandleDriveInput();
    }

    // Assign Movement Inputs
    private void AssignMovementInputs()
    {
        playerControls.PlayerMovement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerControls.PlayerMovement.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void AssignActionInputs()
    {
        playerControls.PlayerActions.Roll.performed += ctx => rollInput = true;
        playerControls.PlayerActions.Roll.canceled += ctx => rollInput = false;
        playerControls.PlayerActions.Sprint.performed += ctx => sprintInput = true;
        playerControls.PlayerActions.Sprint.canceled += ctx => sprintInput = false;
        playerControls.PlayerActions.Jump.performed += ctx => jumpInput = true;
        playerControls.PlayerActions.Jump.canceled += ctx => jumpInput = false;
    }

    private void AssignCameraInput()
    {
        playerControls.PlayerCamera.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerControls.PlayerCamera.Look.canceled += ctx => lookInput = Vector2.zero;
    }
    private void AssignInteractionInputs()
    {
        playerControls.Interactions.EnterVehicle.performed += ctx => enterVehicleInput = true;
        playerControls.Interactions.EnterVehicle.canceled += ctx => enterVehicleInput = false;
    }

    private void AssignDriveInputs()
    {
        playerControls.VehicleControls.Drive.performed += ctx => driveInput = ctx.ReadValue<Vector2>();
        playerControls.VehicleControls.Drive.canceled += ctx => driveInput = Vector2.zero;
        playerControls.VehicleControls.Brake.performed += ctx => brakeVehicleInput = true;
        playerControls.VehicleControls.Brake.canceled += ctx => brakeVehicleInput = false;
    }
    // Assign UI Inputs
    private void AssignUIInputs()
    {
        playerControls.UI.Navigate.performed += ctx => navigationInput = ctx.ReadValue<Vector2>();
        playerControls.UI.Navigate.canceled += ctx => navigationInput = Vector2.zero;
        playerControls.UI.Submit.performed += ctx => submitPerformed = true;
        playerControls.UI.Submit.canceled += ctx => submitPerformed = false;
        playerControls.UI.Delete.performed += ctx => deletePerformed = true;
        playerControls.UI.Delete.canceled += ctx => deletePerformed = false;
        playerControls.UI.Cancel.performed += ctx => cancelPerformed = true;
        playerControls.UI.Cancel.canceled += ctx => cancelPerformed = false;
        playerControls.UI.Click.performed += ctx => clickPerformed = true;
        playerControls.UI.Click.canceled += ctx => clickPerformed = false;
        playerControls.UI.RightClick.performed += ctx => rightClickPerformed = true;
        playerControls.UI.RightClick.canceled += ctx => rightClickPerformed = false;
        playerControls.UI.AnyKey.performed += ctx => anyKeyPerformed = true;
        playerControls.UI.AnyKey.canceled += ctx => anyKeyPerformed = false;
        playerControls.UI.ScrollWheel.performed += ctx => scrollWheelInput = ctx.ReadValue<Vector2>();
        playerControls.UI.ScrollWheel.performed += ctx => scrollWheelInput = Vector2.zero;
    }

    private void HandleMoveInput()
    {
        horizontalMoveInput = moveInput.x;
        verticalMoveInput = moveInput.y;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalMoveInput) + Mathf.Abs(verticalMoveInput));

        if (moveAmount > 0 && moveAmount <= 0.5f)
            moveAmount = 0.5f;  // Walk
        else if (moveAmount > 0.5f)
            moveAmount = 1;     // Run

        // No strafe movement
        if (playerManager)
            playerManager.PlayerAnimationManager.MovementAnimations(0, moveAmount, playerManager.PlayerNetworkManager.isSprinting.Value);

        //// TODO: Enemy locked strafe movement
    }

    private void HandleLookInput()
    {
        horizontalLookInput = lookInput.x;
        verticalLookInput = lookInput.y;
    }

    private void HandleDriveInput()
    {
        horizontalDriveInput = driveInput.x;
        verticalDriveInput = driveInput.y;
    }
    private void HandleDodgeInput()
    {
        if (rollInput)
        {
            rollInput = false;
            playerManager.playerLocomotionManager.AttemptToPerformRoll();
        }
    }

    private void HandleSprintInput()
    {
        if (sprintInput)
        {
            playerManager.playerLocomotionManager.HandleSprinting();
        }
        else if (playerManager != null)
        {
            playerManager.PlayerNetworkManager.isSprinting.Value = false;
        }
    }

    private void HandleJumpInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            playerManager.playerLocomotionManager.AttemptToPerformJump();
        }
    }


    // Enable/Disable Player Movement Input based on the current scene
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 2)
        {
            playerControls.PlayerMovement.Enable();
            playerControls.PlayerActions.Enable();
            playerControls.PlayerCamera.Enable();
            playerControls.Interactions.Enable();
            playerControls.UI.Disable();
        }
        else
        {
            playerControls.PlayerMovement.Disable();
            playerControls.PlayerActions.Disable();
            playerControls.PlayerCamera.Disable();
            playerControls.Interactions.Disable();
            playerControls.UI.Enable();
        }
    }

    // Detect if a gamepad or mouse is connected
    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (device is Gamepad)
        {
            isGamepadActive = true;
            Cursor.visible = false;
            Debug.Log("Gamepad connected");
        }
        else if (device is Pointer)
        {
            isGamepadActive = false;
            Cursor.visible = true;
            Debug.Log("Mouse connected");
        }
    }
}
