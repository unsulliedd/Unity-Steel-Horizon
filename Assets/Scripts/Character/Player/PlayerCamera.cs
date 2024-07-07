using UnityEngine;
using UnityEngine.SceneManagement;
// This script manages the player's camera behavior.
public class PlayerCamera : MonoBehaviour
{
    #region Singleton
    public static PlayerCamera Instance { get; private set; }
    #endregion

    [Header("Camera Components")]
    [HideInInspector] public PlayerManager playerManager;
    public Camera playerCamera;
    [SerializeField] private Transform cameraPivotTransform;

    [Header("Camera Settings")]
    private Vector3 cameraVelocity;                                 // Smooth transition velocity for camera movement
    [SerializeField] private float cameraSmoothing = 1f;            // Smoothing factor for camera movement
    [SerializeField] private float leftRightLookSensitivity = 20f;  // Sensitivity for horizontal look
    [SerializeField] private float upDownLookSensitivity = 10f;     // Sensitivity for vertical look
    [SerializeField] private float minLookAngle = -35f;             // Minimum vertical look angle
    [SerializeField] private float maxLookAngle = 35f;              // Maximum vertical look angle
    [SerializeField] private float cameraHorizontalPosition = 0f;   // Horizontal position of the camera 
    [SerializeField] private float cameraHeight = 1.5f;             // Height of the camera above the player
    [SerializeField] private float cameraDistance = 0;              // Distance between camera and player
    [SerializeField] private Vector3 defaultCameraPosition;         // Default camera position
    private Vector3 newCameraPosition;                              // New camera position
    private float leftRightLookAngle;                               // Current horizontal look angle
    private float upDownLookAngle;                                  // Current vertical look angle

    [Header("Camera Collision")]
    [SerializeField] private float cameraCollisionRadius = 0.2f;    // Radius of the camera collision sphere
    [SerializeField] private LayerMask cameraCollisionLayer;        // Layer mask for camera collision
    private Vector3 cameraObjectPosion;
    private float cameraCollisionZPosition;
    private float targetCollisionZPosition;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerManager = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        // Set default camera position
        defaultCameraPosition = playerCamera.transform.position;
        cameraCollisionZPosition = playerCamera.transform.localPosition.z;

        // Ensure the camera starts at the correct position
        SetCameraPosition();
    }

    public void HandleAllCameraAction()
    {
        if (playerManager != null)
        {
            // Handle all camera-related actions
            CameraFollowTarget();
            CameraRotation();
            CameraCollision();
        }
    }

    public void SetCameraPosition()
    {
        //// TODO: Implement camera position logic in settings menu
        // Adjust camera position to follow player with specified height and distance
        newCameraPosition = new Vector3(cameraHorizontalPosition, cameraHeight, cameraDistance);
        cameraPivotTransform.transform.position = newCameraPosition;
    }

    private void CameraFollowTarget()
    {
        // Smoothly follow the player's position
        Vector3 targetCameraPosition = Vector3.SmoothDamp(
            transform.position,
            playerManager.transform.position,
            ref cameraVelocity,
            cameraSmoothing * Time.deltaTime);

        transform.position = targetCameraPosition;
    }

    private void CameraRotation()
    {
        if (PlayerInputManager.Instance.isGamepadActive)
        {
            leftRightLookSensitivity = 220f;
            upDownLookSensitivity = 200f;
        }
        else
        {
            leftRightLookSensitivity = 20f;
            upDownLookSensitivity = 10f;
        }

        // Adjust camera rotation based on player input
        leftRightLookAngle += (PlayerInputManager.Instance.horizontalLookInput * leftRightLookSensitivity) * Time.deltaTime;
        upDownLookAngle -= (PlayerInputManager.Instance.verticalLookInput * upDownLookSensitivity) * Time.deltaTime;

        // Clamp vertical look angle to prevent over-rotation
        upDownLookAngle = Mathf.Clamp(upDownLookAngle, minLookAngle, maxLookAngle);

        // Apply horizontal rotation
        Vector3 cameraRotation = Vector3.zero;
        cameraRotation.y = leftRightLookAngle;
        Quaternion targetRotation = Quaternion.Euler(cameraRotation);
        transform.rotation = targetRotation;

        // Apply vertical rotation to the camera pivot
        cameraRotation = Vector3.zero;
        cameraRotation.x = upDownLookAngle;
        targetRotation = Quaternion.Euler(cameraRotation);
        cameraPivotTransform.localRotation = targetRotation;
    }

    private void CameraCollision()
    {
        targetCollisionZPosition = cameraCollisionZPosition;
        RaycastHit hit;
        Vector3 direction = playerCamera.transform.position - cameraPivotTransform.position;
        direction.Normalize();

        if (Physics.SphereCast(cameraPivotTransform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetCollisionZPosition), cameraCollisionLayer))
        {
            float distanceFromObject = Vector3.Distance(cameraPivotTransform.position, hit.point);
            targetCollisionZPosition = -distanceFromObject;
        }

        if (Mathf.Abs(targetCollisionZPosition) < cameraCollisionRadius)
            targetCollisionZPosition = -cameraCollisionRadius;

        cameraObjectPosion.z = Mathf.Lerp(playerCamera.transform.localPosition.z, targetCollisionZPosition, cameraCollisionRadius);
        playerCamera.transform.localPosition = cameraObjectPosion;
    }
}
