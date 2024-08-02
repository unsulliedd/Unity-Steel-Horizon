using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

public class BossAgent : Agent
{
    public float bossSpeed = 2.0f;
    public float playerSpeed = 2.0f;
    public float rotateSpeed = 2.0f;
    public float attackDistance = 2.0f;
    public float sphereRadius = 5.0f;
    public Vector3 attackBoxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    private bool canControl, isCompleted;
    public GameObject weapon; // Silahın referansı
    public GameObject[] characters; // 4 karakteri bu diziye ekleyin
    private CharacterController characterController;
    private Animator animator;
    private GameObject closestCharacter;
    private Vector3[] characterDirections;
    private float changeDirectionInterval = 3.0f;
    private float[] timeSinceLastChange;
    private Vector3 attackStartPos;
    private Vector3 attackDirection;
    private bool drawGizmos = false;
    private Vector3 sphereCastStartPos;
    private bool drawSphereGizmos = false;
    public Vector3 minPosition = new Vector3(-10f, 0f, -10f);
    public Vector3 maxPosition = new Vector3(10f, 0f, 10f);
    public float timeDuration;
    private float timer;
    public float gravity = -9.81f;
    private Vector3 velocity;
    private LayerMask playerLayer;
    public override void Initialize()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        characterDirections = new Vector3[characters.Length];
        timeSinceLastChange = new float[characters.Length];
    }

    public override void OnEpisodeBegin()
    {
        timer = timeDuration;
        // Boss pozisyonunu resetleyin
        transform.localPosition = new Vector3(0, 0f, 0);
        canControl = true;
        // Karakterlerin pozisyonlarını rastgele olarak resetleyin
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].transform.localPosition = GetRandomPosition();
            characterDirections[i] = GetRandomDirection();
            timeSinceLastChange[i] = 0.0f;
        }

        isCompleted = false;
        animator.SetBool("Walk", false);
        animator.SetBool("Attack", false);
        FindClosestCharacter();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Boss'un pozisyonunu ve en yakın karakterin pozisyonunu gözlem olarak ekleyin
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(closestCharacter.transform.localPosition);
        sensor.AddObservation(closestCharacter.transform.localPosition - weapon.transform.position);
        sensor.AddObservation(Vector3.Distance(transform.localPosition, closestCharacter.transform.localPosition));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Continuous Actions for Movement and Rotation
        float moveZ = actions.ContinuousActions[0]; // İleri-Geri hareket
        float rotateY = actions.ContinuousActions[1]; // Sağ-Sol rotasyon
        bool attack = actions.DiscreteActions[0] > 0;

       

        Vector3 move = transform.forward * moveZ;
        characterController.Move(move * bossSpeed * Time.deltaTime);

        if (move.magnitude > 0)
        {
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Walk", false);
        }

        // Rotation
        transform.Rotate(0, rotateY * rotateSpeed * Time.deltaTime, 0);
        if(Vector3.Distance(transform.position, closestCharacter.transform.position)<10)
            AddReward(10);

        // Discrete Action for Attack
        if (attack && Vector3.Distance(transform.position, closestCharacter.transform.position) <= attackDistance)
        {
           // Debug.Log("Attempting to attack");
            animator.SetBool("Attack", true);
            PerformAttack();
        }
        else
        {
            animator.SetBool("Attack", false);
        }

        // Karakterleri hareket ettir
        MoveCharacters();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        // Continuous actions
        continuousActions[0] = Input.GetAxis("Vertical"); // İleri-Geri hareket
        continuousActions[1] = Input.GetAxis("Horizontal"); // Sağ-Sol rotasyon

        // Discrete actions
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // Saldırı
    }

    public void triggerControl()
    {
        canControl = true;
    }

    private void Update()
    {
        FindClosestCharacter();
     
        if (!characterController.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0f; // Yere değiyorsa, yerçekimi hızını sıfırla
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    private void PerformAttack()
    {
        AddReward(15);
        //Debug.Log("PerformAttack called");

        FindClosestCharacter();

        if (closestCharacter != null && canControl)
        {
            attackDirection = (closestCharacter.transform.position - weapon.transform.position).normalized;
            attackStartPos = weapon.transform.position;
            drawGizmos = true;

            RaycastHit attackHit;
            if  (Physics.BoxCast(attackStartPos, attackBoxHalfExtents, attackDirection, out attackHit, weapon.transform.rotation, attackDistance, playerLayer))
            {
                if (attackHit.collider.gameObject == closestCharacter)
                {
                    isCompleted = true;
                    AddReward(60);
                    //Debug.Log("Attack successful");
                    EndEpisode();
                    // Ödül ver
                    drawGizmos = false;
                    drawSphereGizmos = false;
                }
                else
                {
                    //Debug.Log("Attack failed: Hit " + attackHit.collider.gameObject.name);
                }
            }
            else
            {
                //Debug.Log("Attack failed: No hit detected");
            }
        }
        else
        {
            //Debug.Log("Attack failed: No closest character or canControl is false");
        }
    }

    private void FindClosestCharacter()
    {
        float minDistance = Mathf.Infinity;
        sphereCastStartPos = transform.position;
        drawSphereGizmos = true;

        RaycastHit[] hits = Physics.SphereCastAll(sphereCastStartPos, sphereRadius, Vector3.forward, 0f);
        foreach (RaycastHit hit in hits)
        {
            GameObject character = hit.collider.gameObject;
            if (Array.Exists(characters, c => c.gameObject == character))
            {
                float distance = Vector3.Distance(transform.position, character.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCharacter = character;
                }
            }
        }

        // Eğer SphereCast ile karakter bulunamazsa, en yakın karakteri Inspector'dan bul
        if (closestCharacter == null)
        {
            foreach (var characterTransform in characters)
            {
                float distance = Vector3.Distance(transform.position, characterTransform.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCharacter = characterTransform.gameObject;
                }
            }
        }
       // Debug.Log(Vector3.Distance(closestCharacter.transform.position,transform.position)+"Mesafe Farkı");
    }

    private void MoveCharacters()
    {
        CheckAndCorrectCharacterPositions(); // Pozisyonları kontrol et

        for (int i = 0; i < characters.Length; i++)
        {
            timeSinceLastChange[i] += Time.deltaTime;

            if (timeSinceLastChange[i] >= changeDirectionInterval)
            {
                characterDirections[i] = GetRandomDirection();
                timeSinceLastChange[i] = 0.0f;
            }

            characters[i].GetComponent<CharacterController>()
                .Move(characterDirections[i] * playerSpeed * Time.deltaTime);
            if (!characters[i].GetComponent<CharacterController>().isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0f; // Yere değiyorsa, yerçekimi hızını sıfırla
            }

            characters[i].GetComponent<CharacterController>().Move(velocity * Time.deltaTime);
        }
    }

    private void CheckAndCorrectCharacterPositions()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            Vector3 position = characters[i].transform.localPosition;

            bool outOfBounds = position.x < minPosition.x || position.x > maxPosition.x ||
                               position.z < minPosition.z || position.z > maxPosition.z;

            if (outOfBounds)
            {
                characterDirections[i] = -characterDirections[i]; // Yönü tersine çevir
                timeSinceLastChange[i] = 0.0f; // Yön değişim süresini sıfırla
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        // Rastgele bir pozisyon döndürür
        float range = 10f; // Örnek aralık değeri, ihtiyaçlarınıza göre ayarlayabilirsiniz
        return new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
    }

    private Vector3 GetRandomDirection()
    {
        float range = 1.0f;
        return new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range)).normalized;
    }

  

    public void OnAttackAnimationEnd()
    {
        if (!isCompleted)
        {
           // Debug.Log("Attack failed");
            AddReward(-10);
            EndEpisode(); // Ceza ver
        }

        drawGizmos = false;
        drawSphereGizmos = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.gameObject.CompareTag("Wall"))
        {
            AddReward(-15);
            EndEpisode();
        }
    }
}
