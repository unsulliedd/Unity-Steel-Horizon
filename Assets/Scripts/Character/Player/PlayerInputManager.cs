using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("Look Input")]
    [SerializeField] private Vector2 lookInput;
    public float horizontalLookInput;
    public float verticalLookInput;

    // UI Actions
    [Header("UI Input")]
    public Vector2 navigationInput;
    public Vector2 pointerPosition;
    public bool submitPerformed;
    public bool cancelPerformed;
    public bool clickPerformed;
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
        playerControls.PlayerCamera.Disable();
        playerControls.UI.Enable();

        AssignMovementInputs();
        AssignCameraInput();
        AssignUIInputs();
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
    }

    // Assign Movement Inputs
    private void AssignMovementInputs()
    {

        playerControls.PlayerMovement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerControls.PlayerMovement.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void AssignCameraInput()
    {
        playerControls.PlayerCamera.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerControls.PlayerCamera.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    // Assign UI Inputs
    private void AssignUIInputs()
    {
        playerControls.UI.Navigate.performed += ctx => navigationInput = ctx.ReadValue<Vector2>();
        playerControls.UI.Navigate.canceled += ctx => navigationInput = Vector2.zero;
        playerControls.UI.Submit.performed += ctx => submitPerformed = true;
        playerControls.UI.Submit.canceled += ctx => submitPerformed = false;
        playerControls.UI.Cancel.performed += ctx => cancelPerformed = true;
        playerControls.UI.Cancel.canceled += ctx => cancelPerformed = false;
        playerControls.UI.Point.performed += ctx => pointerPosition = ctx.ReadValue<Vector2>();
        playerControls.UI.Point.canceled += ctx => pointerPosition = Vector2.zero;
        playerControls.UI.Click.performed += ctx => clickPerformed = true;
        playerControls.UI.Click.canceled += ctx => clickPerformed = false;
        playerControls.UI.AnyKey.performed += ctx => anyKeyPerformed = true;
        playerControls.UI.AnyKey.canceled += ctx => anyKeyPerformed = false;
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
    }

    private void HandleLookInput()
    {
        horizontalLookInput = lookInput.x;
        verticalLookInput = lookInput.y;
    }

    // Enable/Disable Player Movement Input based on the current scene
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 1)
        {
            playerControls.PlayerMovement.Enable();
            playerControls.PlayerCamera.Enable();
            playerControls.UI.Disable();
        }
        else
        {
            playerControls.PlayerMovement.Disable();
            playerControls.PlayerCamera.Disable();
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
