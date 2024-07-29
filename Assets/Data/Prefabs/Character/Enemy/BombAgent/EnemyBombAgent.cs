using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBombAgent : Agent, INetworkSerializable
{
    [Header("Character Parameters")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRadius = 10f; // Tarama yarıçapı
    [SerializeField] private Transform player;

    [SerializeField] float explodeDuration;
    private Rigidbody enemyMovement;
    float time = 0;

    public Material material; // Rengini değiştirmek istediğiniz materyal
    public Color targetColor = Color.red; // Hedef renk (kırmızı)
    public float duration = 1f; // Renk değişiminin süresi
    public float bombTriggerTime = 10f; // Bomba tetiklenme süresi
    private bool isBombTriggered;
    private Color originalColor; // Orijinal renk
    private float bombTimer; // Bomba tetikleme süresi sayacı

    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>();

    public override void Initialize()
    {
        enemyMovement = GetComponent<Rigidbody>();
        if (material != null)
        {
            originalColor = material.color;
        }
        if (IsServer)
        {
            networkedPosition.Value = transform.position;
            networkedRotation.Value = transform.rotation;
        }
    }

    public override void OnEpisodeBegin()
    {
     
            enemyMovement.velocity = Vector3.zero;
            transform.localPosition = Vector3.zero;
            StopAllCoroutines();
            bombTimer = bombTriggerTime; // Bombayı tetiklemek için sayaç sıfırlanır
            isBombTriggered = true;
            time = 0;

            // Player referansını güncelle
            UpdateNearestPlayer();
       
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(player.localPosition);
        //sensor.AddObservation(Vector3.Distance(transform.localPosition, player.localPosition));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
       // if (!IsServer) return;

        float move_Rotate = actions.ContinuousActions[0];
        float move_Forward = actions.ContinuousActions[1];
        bool shoot = actions.DiscreteActions[0] > 0;

        enemyMovement.velocity = transform.forward * (move_Forward * moveSpeed * Time.deltaTime);
        transform.Rotate(0, move_Rotate * moveSpeed * Time.deltaTime, 0);
        transform.GetChild(0).Rotate(move_Forward * moveSpeed * Time.deltaTime, 0, 0);

        networkedPosition.Value = transform.position;
        networkedRotation.Value = transform.rotation;

        if (player != null)
        {
            if (Vector3.Distance(transform.position, player.position) < 2)
            {
                if (shoot)
                {
                    TriggerBombClient();
                }
            }
        } 
   
    }

  
    private void TriggerBombClient()
    {
        StartCoroutine(TriggerBomb());
    }

    IEnumerator TriggerBomb()
    {
        AddReward(5); // Oyuncuyu bulup bombayı tetiklediği için ödüllendirin.

        while (time < explodeDuration)
        {
            yield return StartCoroutine(ChangeToColor(originalColor, targetColor, duration));
            yield return StartCoroutine(ChangeToColor(targetColor, originalColor, duration));
            time += Time.deltaTime;
        }

        if (Vector3.Distance(transform.position, player.position) < 5f)
        {
            AddReward(30);
        }
        else
        {
            AddReward(-5);
        }

        EndEpisode(); // Bölümü sonlandır
    }

    IEnumerator ChangeToColor(Color startColor, Color endColor, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            if (material != null)
            {
                material.color = Color.Lerp(startColor, endColor, t);
            }

            yield return null;
        }

        if (material != null)
        {
            material.color = endColor;
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
       // if (!IsServer) return;

        bombTimer -= Time.fixedDeltaTime;
        if (bombTimer <= 0)
        {
            AddReward(-15f);
            StopAllCoroutines();
            material.color = originalColor;
            EndEpisode();
        }
        enemyMovement.AddForce(Physics.gravity, ForceMode.Acceleration);
        UpdateNearestPlayer();
    }

    private void Update()
    {
       /* if (IsServer)
        {
            transform.position = networkedPosition.Value;
            transform.rotation = networkedRotation.Value;
        }*/

     
          
        
    }

    private void UpdateNearestPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        float nearestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPlayer = hitCollider.transform;
                }
            }
        }

        if (nearestPlayer != null)
        {
            player = nearestPlayer;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!IsServer) return;

        if (other.collider.CompareTag("Wall"))
        {
            AddReward(-15f);
            EndEpisode();
        }
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

    private bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
}
