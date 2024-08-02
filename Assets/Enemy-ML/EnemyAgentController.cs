using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Netcode;
using UnityEngine;

public class EnemyAgentController : Agent, INetworkSerializable
{
    [Header("Character Parameters")]
    [SerializeField] private float moveSpeed = 2f;
    private CharacterController enemyMovement;
    [SerializeField] private GunController gunObject;
    private bool can_shoot, hit_target, has_shot = false;
    private int time_until_next_bullet = 0;
    private int min_time_until_next_bullet = 120;
    private Animator _animator;
    private bool isMoving;
    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>();

    public override void Initialize()
    {
        _animator = GetComponent<Animator>();
        enemyMovement = GetComponent<CharacterController>();

        if (IsServer)
        {
            networkedPosition.Value = transform.position;
            networkedRotation.Value = transform.rotation;
        }
    }

    public override void OnEpisodeBegin()
    {
        if (IsServer)
        {
            // Reset position and state on the server
            transform.position = networkedPosition.Value;
            transform.rotation = networkedRotation.Value;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!IsServer) return; // Ensure only the server processes actions

        can_shoot = false;
        float move_Rotate = actions.ContinuousActions[0];
        float move_Forward = actions.ContinuousActions[1];
        bool shoot = actions.DiscreteActions[0] > 0;
        enemyMovement.Move(transform.forward * (move_Forward * moveSpeed * Time.deltaTime));
        enemyMovement.transform.Rotate(0f, move_Rotate, 0f, Space.Self);

        networkedPosition.Value = transform.position;
        networkedRotation.Value = transform.rotation;

        if (shoot && !has_shot)
        {
            can_shoot = true;
        }

        if (can_shoot)
        {
            hit_target = gunObject.ShootGun();

            time_until_next_bullet = min_time_until_next_bullet;
            has_shot = true;
            if (hit_target)
            {
                AddReward(30);
            }
            else
            {
                AddReward(-1);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousAction = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        continuousAction[0] = Input.GetAxis("Horizontal");
        continuousAction[1] = Input.GetAxis("Vertical");
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private void FixedUpdate()
    {
        //if (!IsServer) return; // Ensure only the server processes FixedUpdate

        if (has_shot)
        {
            time_until_next_bullet--;
            if (time_until_next_bullet <= 0)
            {
                has_shot = false;
            }
        }
    }

    private void Update()
    {
        if (enemyMovement.velocity != Vector3.zero)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if (!IsServer)
        {
            // Update position and rotation based on the networked values
            transform.position = networkedPosition.Value;
            transform.rotation = networkedRotation.Value;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!IsServer) return; // Ensure only the server processes collisions

        if (hit.collider.CompareTag("Wall"))
        {
            AddReward(-15f);
        }
        else if (hit.collider.CompareTag("Player"))
        {
            AddReward(-15f);
        }
    }

    private void OnAnimatorMove()
    {
        _animator.SetBool("canMove", isMoving);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        Vector3 position = networkedPosition.Value;
        Quaternion rotation = networkedRotation.Value;

        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);

        if (serializer.IsReader)
        {
            networkedPosition.Value = position;
            networkedRotation.Value = rotation;
        }
    }

    private bool IsServer => NetworkManager.Singleton.IsServer;
}