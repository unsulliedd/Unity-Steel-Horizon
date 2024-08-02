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
    // Bomba tetiklenme süresi
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
        if (IsServer)
        {
            StopAllCoroutines();
          
            isBombTriggered = true;
            time = 0;

            // Player referansını güncelle
            UpdateNearestPlayer();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(player.localPosition);
        sensor.AddObservation(Vector3.Distance(transform.localPosition, player.localPosition));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!IsServer) return;

        float move_Rotate = actions.ContinuousActions[0];
        float move_Forward = actions.ContinuousActions[1];
        bool shoot = actions.DiscreteActions[0] > 0;

        enemyMovement.velocity = transform.forward * (move_Forward * moveSpeed * Time.deltaTime);
        transform.Rotate(0, move_Rotate * moveSpeed * Time.deltaTime, 0);
        transform.GetChild(0).Rotate(move_Forward * moveSpeed * Time.deltaTime, 0, 0);

        networkedPosition.Value = transform.position;
        networkedRotation.Value = transform.rotation;

        if (Vector3.Distance(transform.position, player.position) < 2)
        {
            if (shoot)
            {
                TriggerBombClientRpc();
            }
        }
    }

    [ClientRpc]
    private void TriggerBombClientRpc()
    {
        StartCoroutine(TriggerBomb());
    }

    IEnumerator TriggerBomb()
    {
      

        while (time < explodeDuration)
        {
            yield return StartCoroutine(ChangeToColor(originalColor, targetColor, duration));
            yield return StartCoroutine(ChangeToColor(targetColor, originalColor, duration));
            time += Time.deltaTime;
        }
        
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
        if (!IsServer) return;
        enemyMovement.AddForce(Physics.gravity*300,ForceMode.Acceleration);

      
        if (bombTimer <= 0)
        {
         
            StopAllCoroutines();
            material.color = originalColor;
         
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            transform.position = networkedPosition.Value;
            transform.rotation = networkedRotation.Value;
        }

        if (IsServer)
        {
            UpdateNearestPlayer();
        }
    }

    private void UpdateNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float nearestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;

        // Her bir "Player" nesnesi için döngü
        foreach (var obj in players)
        {
            // Mevcut obje ile oyuncu arasındaki mesafeyi hesaplayın
            float distance = Vector3.Distance(transform.position, obj.transform.position);

            // Eğer bu mesafe, şimdiye kadar bulunan en yakın mesafeden daha küçükse
            if (distance < nearestDistance)
            {
                // En yakın mesafeyi ve oyuncuyu güncelleyin
                nearestDistance = distance;
                nearestPlayer = obj.transform;
            }
        }

        // Eğer bir oyuncu bulunduysa, player değişkenini güncelleyin
        if (nearestPlayer != null)
        {
            player = nearestPlayer;
            Debug.Log("Nearest player: " + player.name);
        }
    
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!IsServer) return;

  
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
