using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Manages the MonoWheel vehicle, including driving mechanics and player interactions.
/// </summary>
public class MonoWheelManager : MonoBehaviour
{
    public static MonoWheelManager Instance;

    [Header("Player Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform seatPosition;
    [SerializeField] private Transform exitPosition;

    [Header("Vehicle Settings")]
    [SerializeField] private float vehicleSpeed;
    [SerializeField] private float vehicleTurningSpeed;
    [SerializeField] private float maxSpeed = 120f;
    [SerializeField] private float stabilizationForce;
    [SerializeField] private Transform wheelTransform;
    [SerializeField] private float wheelRadius = 0.5f;
    [SerializeField] private GameObject wheel;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckSphereRadius = 0.1f;
    [SerializeField] private LayerMask groundCheckLayerMask;
    private bool isGrounded;

    [Header("Effects")]
    [SerializeField] private ParticleSystem smokeEffect1;
    [SerializeField] private ParticleSystem smokeEffect2;

    [Header("Volume Settings")]
    [SerializeField] private Volume volume;
    private MotionBlur motionBlurEffect;

    public PlayerManager playerManager;
    private Rigidbody rigidBody;

    private bool isDriving;
    private float distanceToPlayer;
    private const float cooldownTime = 0.5f;
    private float nextActionTime;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Time.time > nextActionTime)
        {
            if (PlayerInputManager.Instance.enterVehicleInput)
            {
                if (isDriving)
                {
                    ExitVehicle();
                    StopSmokeEffects();
                    playerManager.transform.SetParent(null);
                }
                else if (IsPlayerNearVehicle())
                {
                    EnterVehicle();
                    playerManager.transform.SetParent(transform);
                }
                nextActionTime = Time.time + cooldownTime;
            }

            if (volume.profile.TryGet(out motionBlurEffect) && volume != null)
            {
                motionBlurEffect.active = false;
            }
        }

        if (isDriving)
        {
            Drive();
            PositionPlayerInSeat();
        }
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (!isGrounded)
        {
            ApplyAirControl();
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckSphereRadius, groundCheckLayerMask);
    }

    private void ApplyAirControl()
    {
        Vector3 stabilizationForceVector = Vector3.down * stabilizationForce;
        rigidBody.AddForce(stabilizationForceVector, ForceMode.Acceleration);
        rigidBody.angularVelocity = Vector3.Lerp(rigidBody.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 2f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckSphereRadius);
    }

    private void Drive()
    {
        playerManager.Animator.SetBool("isDrive", true);

        if (rigidBody.velocity.magnitude < maxSpeed)
        {
            rigidBody.AddForce(transform.forward * PlayerInputManager.Instance.verticalDriveInput * vehicleSpeed, ForceMode.Acceleration);
            StartSmokeEffects();
        }

        Quaternion rotation = Quaternion.Euler(0, PlayerInputManager.Instance.horizontalDriveInput * Time.deltaTime * vehicleTurningSpeed, 0);
        rigidBody.MoveRotation(rotation * rigidBody.rotation);

        RotateWheel();

        if (PlayerInputManager.Instance.brakeVehicleInput)
            ApplyBrakes();
    }

    private void RotateWheel()
    {
        float speed = rigidBody.velocity.magnitude;
        float wheelCircumference = 2 * Mathf.PI * wheelRadius;
        float rotationAngle = (speed * 360) / wheelCircumference;
        wheel.transform.Rotate(Vector3.back, rotationAngle * Time.deltaTime);
    }

    private void ApplyBrakes()
    {
        rigidBody.velocity = Vector3.Lerp(rigidBody.velocity, Vector3.zero, Time.deltaTime);
        rigidBody.angularVelocity = Vector3.zero;
    }

    private void ExitVehicle()
    {
        isDriving = false;
        EnablePlayerControls();
        playerManager.Animator.SetBool("isDrive", false);
        playerManager.transform.SetPositionAndRotation(exitPosition.position, exitPosition.rotation);
    }

    private bool IsPlayerNearVehicle()
    {
        distanceToPlayer = Vector3.Distance(transform.position, playerManager.transform.position);
        return distanceToPlayer < 2.0f;
    }

    private void EnterVehicle()
    {
        isDriving = true;
        DisablePlayerControls();
    }

    private void PositionPlayerInSeat()
    {
        playerManager.transform.SetPositionAndRotation(seatPosition.position, seatPosition.rotation);
    }

    private void StartSmokeEffects()
    {
        if (!smokeEffect1.isPlaying)
            smokeEffect1.Play();
        if (!smokeEffect2.isPlaying)
            smokeEffect2.Play();
    }

    private void StopSmokeEffects()
    {
        smokeEffect1.Stop();
        smokeEffect2.Stop();
    }

    private void EnablePlayerControls()
    {
        var controls = PlayerInputManager.Instance.playerControls;
        controls.PlayerMovement.Enable();
        controls.PlayerCamera.Enable();
        controls.PlayerActions.Enable();
        controls.PlayerCombat.Enable();
        controls.VehicleControls.Disable();
    }

    private void DisablePlayerControls()
    {
        var controls = PlayerInputManager.Instance.playerControls;
        controls.PlayerMovement.Disable();
        controls.PlayerCamera.Disable();
        controls.PlayerActions.Disable();
        controls.PlayerCombat.Disable();
        controls.VehicleControls.Enable();
    }
}
