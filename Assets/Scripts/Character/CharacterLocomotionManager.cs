using System;
using UnityEngine;

public class CharacterLocomotionManager : MonoBehaviour
{
    #region References
    private CharacterManager characterManager;
    #endregion

    [Header("Ground Check")]
    [SerializeField] protected float groundCheckSphereRadius = 0.25f;
    [SerializeField] protected Transform groundCheckSphere;
    [SerializeField] protected LayerMask groundCheckLayerMask;

    [Header("Velocity")]
    protected Vector3 yVelocity;
    protected float inAirTimer = 0f;
    [SerializeField] protected float groundedYVelocity = -20f;
    [SerializeField] protected float fallStartingVelocity = -5f;
    [SerializeField] protected float gravity = -9.81f;
    [SerializeField] protected bool fallingVelocityHasBeenSet = false;

    // Initialize characterManager
    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Handle ground and air states
        HandleGrounding();
        HandleAirborne();

        characterManager.CharacterController.Move(yVelocity * Time.deltaTime);
    }

    // FixedUpdate is called at fixed intervals for physics updates
    protected virtual void FixedUpdate()
    {
        CheckGround();
    }

    // Handle the grounding logic
    private void HandleGrounding()
    {
        if (characterManager.isGrounded)
        {
            if (yVelocity.y < 0)
            {
                inAirTimer = 0;
                fallingVelocityHasBeenSet = false;
                yVelocity.y = groundedYVelocity;
            }
        }
    }

    // Handle the airborne logic
    private void HandleAirborne()
    {
        if (!characterManager.isGrounded)
        {
            if (characterManager.isJumping && !fallingVelocityHasBeenSet)
            {
                yVelocity.y = fallStartingVelocity;
                fallingVelocityHasBeenSet = true;
            }

            inAirTimer += Time.deltaTime;
            characterManager.Animator.SetFloat("InAirTimer", inAirTimer);

            yVelocity.y += gravity * Time.deltaTime;
        }
    }

    // Check if the character is grounded
    private void CheckGround()
    {
        characterManager.isGrounded = Physics.CheckSphere(groundCheckSphere.position, groundCheckSphereRadius, groundCheckLayerMask);
    }

    // Draw ground check sphere in the editor
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckSphere.position, groundCheckSphereRadius);
    }
}
