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
    [SerializeField] public Vector2 lookInput;
    public float horizontalLookInput;
    public float verticalLookInput;

    [Header("Drive Input")]
    [SerializeField] private Vector2 driveInput;
    public float horizontalDriveInput;
    public float verticalDriveInput;
    public bool brakeVehicleInput;

    [Header("Interaction Input")]
    public bool interactInput;

    [Header("Combat Input")]
    public bool fireInput;
    public bool aimInput;
    public bool combatMode;

    [Header("Player UI Input")]
    public bool openCharacterMenuInput;
    public bool openInventoryMenuInput;
    public bool openSkillMenuInput;

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
        playerControls.PlayerCombat.Disable();
        playerControls.PlayerInteractions.Disable();
        playerControls.PlayerUI.Disable();
        playerControls.VehicleControls.Disable();
        playerControls.UI.Enable();

        AssignMovementInputs();
        AssignActionInputs();
        AssignCameraInput();
        AssignCombatInput();
        AssignUIInputs();
        AssignDriveInputs();
        AssignInteractionInputs();
        AssignPlayerUIInputs();
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
        HandleAimInput();
        HandleFireInput();
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
        playerControls.PlayerInteractions.Interact.performed += ctx => interactInput = true;
        playerControls.PlayerInteractions.Interact.canceled += ctx => interactInput = false;
    }

    private void AssignDriveInputs()
    {
        playerControls.VehicleControls.Drive.performed += ctx => driveInput = ctx.ReadValue<Vector2>();
        playerControls.VehicleControls.Drive.canceled += ctx => driveInput = Vector2.zero;
        playerControls.VehicleControls.Brake.performed += ctx => brakeVehicleInput = true;
        playerControls.VehicleControls.Brake.canceled += ctx => brakeVehicleInput = false;
    }
    private void AssignCombatInput()
    {
        playerControls.PlayerCombat.Fire.performed += ctx => fireInput = true;
        playerControls.PlayerCombat.Fire.canceled += ctx => fireInput = false;
        playerControls.PlayerCombat.Aim.performed += ctx => aimInput = true;
        playerControls.PlayerCombat.Aim.canceled += ctx => aimInput = false;
        playerControls.PlayerCombat.CombatMode.performed += ctx => ToggleCombatMode();
    }

    private void AssignPlayerUIInputs()
    {
        playerControls.PlayerUI.OpenMenu.performed += ctx => ToggleOpenCharacterMenu();
        playerControls.PlayerUI.OpenInventory.performed += ctx => ToggleInventoryMenu();
        playerControls.PlayerUI.OpenSkills.performed += ctx => ToggleSkillMenu();
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

        if (playerManager)
            playerManager.PlayerAnimationManager.MovementAnimations(0, moveAmount, playerManager.PlayerNetworkManager.isSprinting.Value);
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
            playerManager.playerLocomotionManager.HandleSprinting();
        else if (playerManager != null)
            playerManager.PlayerNetworkManager.isSprinting.Value = false;
    }

    private void HandleJumpInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            playerManager.playerLocomotionManager.AttemptToPerformJump();
        }
    }

    private void HandleAimInput()
    {
        if (aimInput && combatMode)
            playerManager.PlayerCombatManager.HandleAiming();
    }

    private void HandleFireInput()
    {
        if (fireInput)
        {
            fireInput = false;
            playerManager.PlayerCombatManager.HandleShooting();
        }
    }

    private void ToggleCombatMode() => combatMode = !combatMode;

    private void ToggleOpenCharacterMenu() => openCharacterMenuInput = !openCharacterMenuInput;
    private void ToggleInventoryMenu() => openInventoryMenuInput = !openInventoryMenuInput;
    private void ToggleSkillMenu() => openSkillMenuInput = !openSkillMenuInput;

    // Enable/Disable Player Movement Input based on the current scene
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == SaveGameManager.Instance.GetWorldSceneIndex())
        {
            playerControls.PlayerMovement.Enable();
            playerControls.PlayerActions.Enable();
            playerControls.PlayerCamera.Enable();
            playerControls.PlayerCombat.Enable();
            playerControls.PlayerInteractions.Enable();
            playerControls.PlayerUI.Enable();
            playerControls.UI.Disable();
        }
        else
        {
            playerControls.PlayerMovement.Disable();
            playerControls.PlayerActions.Disable();
            playerControls.PlayerCamera.Disable();
            playerControls.PlayerCombat.Disable();
            playerControls.PlayerInteractions.Disable();
            playerControls.PlayerUI.Disable();
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
            //Debug.Log("Gamepad connected");
        }
        else if (device is Pointer)
        {
            isGamepadActive = false;
            Cursor.visible = true;
            //Debug.Log("Mouse connected");
        }
    }
}