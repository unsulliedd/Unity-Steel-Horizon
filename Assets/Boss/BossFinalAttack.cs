using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossFinalAttack : MonoBehaviour
{
    public float dashAttackInterval = 15.0f; // Dash saldırısı aralığı
    private float dashAttackTimer;
    public float dashAttackSpeed = 20.0f; // Dash saldırı hızı
    public float attackRange = 2.0f; // Saldırı mesafesi
    public float sphereRadius = 5.0f; // Taramak için küre yarıçapı
    public LayerMask characterLayer; // Karakterlerin katmanı
    public float rotationSpeed = 2.0f; // Dönüş hızı
    private Animator animator; // Boss'un animatörü
    private NavMeshAgent agent; // Boss'un karakter kontrolcüsü
    public GameObject dashEffect; // Dash sırasında çalışacak efekt
    public float followDuration = 3.0f; // Dash saldırısından önce oyuncuyu takipleme süresi
    public float dashAcceleration = 50.0f; // Dash sırasında ivme
    public float baseOffsetIncrease = 1.0f; // Dash sırasında base offset artışı
    public float baseOffsetDuration = 0.5f;
    private float normalSpeed,normalAcceleration;
    private Transform playerPos; // Base offset artış süresi
    public bool canDashAttack;

    private void Start()
    {
        dashAttackTimer = dashAttackInterval;
        animator = GetComponent<Animator>(); // Boss'un animatörünü al
        agent = GetComponent<NavMeshAgent>(); // NavMeshAgent bileşenini al
        normalSpeed = agent.speed;
        normalAcceleration = agent.acceleration;
    }

    public void PerformDashAttack()
    {
        // Küre taraması yaparak oyuncuları bul
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float farthestDistance = 0f;
        GameObject farthestPlayer = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance > farthestDistance)
            {
                farthestDistance = distance;
                farthestPlayer = player;
                
            }
        }

        if (farthestPlayer != null)
        {
            playerPos = farthestPlayer.transform;
            StartCoroutine(FollowAndDashAttack(farthestPlayer));
        }
    }

    private IEnumerator FollowAndDashAttack(GameObject targetCharacter)
    {

        // Takip tamamlandıktan sonra Dash saldırısını başlat
        if (dashEffect != null)
        {
            dashEffect.SetActive(true);
        }
        

        // Dash animasyonunu oynat
        animator.SetTrigger("Dash");
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetCharacter.transform.position;

        // Hedefe doğru hızla ilerleme
        agent.speed = dashAttackSpeed;
        agent.acceleration = dashAcceleration;
        StartCoroutine(AdjustBaseOffset());

        agent.SetDestination(targetPos);

        while (!agent.pathPending && agent.remainingDistance > attackRange)
        {
            yield return null;
        }

        // Hedef karaktere yakınsa saldırı yap
        yield return new WaitUntil(() => Vector3.Distance(transform.position, targetCharacter.transform.position) <= attackRange);
        canDashAttack = true;
        animator.SetBool("DashAttack",canDashAttack);

        // Dash saldırısını gerçekleştir

    }

    private void Update()
    {
        if (canDashAttack)
        {
            agent.acceleration = normalAcceleration;
            agent.speed = normalSpeed;
            canDashAttack = false;
        }
    }


    private IEnumerator AdjustBaseOffset()
    {
        float originalBaseOffset = agent.baseOffset;
        agent.baseOffset += baseOffsetIncrease;

        yield return new WaitForSeconds(baseOffsetDuration);

        agent.baseOffset = originalBaseOffset;
    }

    private void OnDrawGizmos()
    {
        // SphereCast alanını çizdir
        Gizmos.color = Color.red; // Alanın rengini ayarla
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }

    private void OnAnimatorMove()
    {
        
    }
}
