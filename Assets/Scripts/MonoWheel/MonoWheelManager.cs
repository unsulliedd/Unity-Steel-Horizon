using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Manages the MonoWheel vehicle, including driving mechanics and player interactions.
/// </summary>
public class MonoWheelManager : MonoBehaviour
{
    #region References
    [Header("References")]
    private PlayerManager playerManager;
    private Rigidbody rigidBody;
    #endregion

    [Header("Player Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform seatPosition;
    [SerializeField] private Transform exitPosition;

    [Header("MonoWheel Settings")]
    [SerializeField] private float monoWheelSpeed;
    [SerializeField] private float monoWheelTurningSpeed;
    [SerializeField] private float monoWheelMaxSpeed = 120f;
    [SerializeField] private float stabilizationForce;
    [SerializeField] private float wheelRadius = 0.5f;
    [SerializeField] private Transform wheelTransform;

    [SerializeField] private bool isDriving = false;
    [SerializeField] private float distanceToPlayer;
    [SerializeField] private float cooldownTime = 0.5f;
    [SerializeField] private float nextActionTime;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckSphereRadius = 0.1f;
    [SerializeField] private bool isGrounded;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundCheckLayerMask;

    [Header("Effects")]
    [SerializeField] private ParticleSystem smokeEffectLeft;
    [SerializeField] private ParticleSystem smokeEffectRight;

    [Header("Volume Settings")]
    [SerializeField] private Volume volume;
    [SerializeField] private MotionBlur motionBlurEffect;


    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(FindPlayer());
        volume.profile.TryGet<MotionBlur>(out motionBlurEffect);
    }
    private void Update()
    {
        if (playerManager == null)
            return;

        if (Time.time > nextActionTime)
        {
            if (PlayerInputManager.Instance.interactInput)
            {
                if (isDriving)
                {
                    ExitVehicle();
                    StopSmokeEffects();
                }
                else if (IsPlayerNearVehicle())
                    EnterVehicle();

                nextActionTime = Time.time + cooldownTime;
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
            ApplyAirControl();
    }


    private void Drive()
    {
        if (playerManager == null)
            return;

        playerManager.Animator.SetBool("isDrive", isDriving);

        if (rigidBody.velocity.magnitude < monoWheelMaxSpeed)
        {
            rigidBody.AddForce(monoWheelSpeed * PlayerInputManager.Instance.verticalDriveInput * transform.forward, ForceMode.Acceleration);
            StartSmokeEffects();
        }

        Quaternion rotation = Quaternion.Euler(0, PlayerInputManager.Instance.horizontalDriveInput * Time.deltaTime * monoWheelTurningSpeed, 0);
        rigidBody.MoveRotation(rotation * rigidBody.rotation);

        RotateWheel();

        if (PlayerInputManager.Instance.brakeVehicleInput)
            ApplyBrakes();
    }

    private void ApplyAirControl()
    {
        Vector3 stabilizationForceVector = Vector3.down * stabilizationForce;
        rigidBody.AddForce(stabilizationForceVector, ForceMode.Acceleration);
        rigidBody.angularVelocity = Vector3.Lerp(rigidBody.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 2f);
    }
    private void RotateWheel()
    {
        float speed = rigidBody.velocity.magnitude;
        float wheelCircumference = 2 * Mathf.PI * wheelRadius;
        float rotationAngle = (speed * 360) / wheelCircumference;
        wheelTransform.Rotate(Vector3.back, rotationAngle * Time.deltaTime);
    }

    private void ApplyBrakes()
    {
        rigidBody.velocity = Vector3.Lerp(rigidBody.velocity, Vector3.zero, Time.deltaTime);
        rigidBody.angularVelocity = Vector3.zero;
    }
    private void EnterVehicle()
    {
        isDriving = true;
        playerManager.CharacterController.enabled = false;
        DisablePlayerControls();
        motionBlurEffect.active = false;
    }

    private void ExitVehicle()
    {
        isDriving = false;
        playerManager.CharacterController.enabled = true;
        EnablePlayerControls();
        motionBlurEffect.active = true;
        playerManager.Animator.SetBool("isDrive", isDriving);
        playerManager.transform.SetPositionAndRotation(exitPosition.position, exitPosition.rotation);
    }

    private bool IsPlayerNearVehicle()
    {
        distanceToPlayer = Vector3.Distance(transform.position, playerManager.transform.position);
        return distanceToPlayer < 2.0f;
    }

    private void PositionPlayerInSeat()
    {
        playerManager.transform.SetPositionAndRotation(seatPosition.position, seatPosition.rotation);
    }

    private void StartSmokeEffects()
    {
        if (!smokeEffectLeft.isPlaying)
            smokeEffectLeft.Play();
        if (!smokeEffectRight.isPlaying)
            smokeEffectRight.Play();
    }

    private void StopSmokeEffects()
    {
        smokeEffectLeft.Stop();
        smokeEffectRight.Stop();
    }

    private void EnablePlayerControls()
    {
        PlayerInputManager.Instance.playerControls.PlayerMovement.Enable();
        PlayerInputManager.Instance.playerControls.PlayerCamera.Enable();
        PlayerInputManager.Instance.playerControls.PlayerActions.Enable();
        PlayerInputManager.Instance.playerControls.PlayerCombat.Enable();
        PlayerInputManager.Instance.playerControls.VehicleControls.Disable();
    }

    private void DisablePlayerControls()
    {
        PlayerInputManager.Instance.playerControls.PlayerMovement.Disable();
        PlayerInputManager.Instance.playerControls.PlayerCamera.Disable();
        PlayerInputManager.Instance.playerControls.PlayerActions.Disable();
        PlayerInputManager.Instance.playerControls.PlayerCombat.Disable();
        PlayerInputManager.Instance.playerControls.VehicleControls.Enable();
    }
    private void CheckGround() => isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckSphereRadius, groundCheckLayerMask);

    IEnumerator FindPlayer()
    {
        yield return new WaitForSeconds(5f);
        playerManager = GameObject.FindWithTag("Player").GetComponent<PlayerManager>();
    }
}
