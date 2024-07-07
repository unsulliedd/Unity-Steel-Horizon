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
    #endregion

    // UI Actions
    [Header("UI Actions")]
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

        playerControls.UI.Enable();

        AssignUIInputs();
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        InputSystem.onEvent -= OnInputEvent;
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

    // Enable/Disable Player Movement Input based on the current scene
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 1)
        {
            playerControls.UI.Disable();
        }
        else
        {
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
