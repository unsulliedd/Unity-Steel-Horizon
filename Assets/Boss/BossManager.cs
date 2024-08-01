using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class BossManager : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float growthScaleWave, growthScaleWaveEffect;
    public Transform shield;
    public float attackRange = 5f; // Saldırı menzili
    public float attackCooldown = 2f; // Saldırı arası bekleme süresi
    public float basicAttackDuration = 5f; // Basic attack süresi
    public float defendDuration = 3f; // Defend süresi
    private float attackTimer;
    private float basicAttackTimer;
    private float defendTimer;
    private BossFinalAttack thirdAttack;
    private BossAreaAttack secondAttack;
    private UnityEngine.AI.NavMeshAgent agent;
    public Transform target;
    private Animator animator;
    public bool takingContinuousDamage = false;
    private float damageTakenTime = 0f;
    private float damageThreshold = 2f; // Sürekli damage yeme süresi
    [SerializeField] private int maxBasicAttack;
    [SerializeField] private GameObject wave,waveEffect;
    private bool canSetPos;
    private float waveScale,waveEffectScale;
    enum BossState { Idle, BasicAttack, Defend, Attacking }
    private BossState state;
    private Vector3 retreatPos;
    public Transform origin;
   

    // Kutunun yarı genişlik, yarı yükseklik ve yarı derinlik değerleri
    public Vector3 halfExtents = new Vector3(1, 1, 1);

    // BoxCast'in yönü
    public Vector3 direction = Vector3.forward;

    // BoxCast'in ne kadar uzağa gitmesini istediğiniz
    public float maxDistance = 10f;

    // Hangi katmanların BoxCast ile kontrol edileceğini belirler
    public LayerMask layerMask;
    void Start()
    {
        waveScale = 1;
        waveEffectScale = 1;
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animator = GetComponent<Animator>();
        secondAttack = GetComponent<BossAreaAttack>();
        thirdAttack = GetComponent<BossFinalAttack>();
        state = BossState.Idle;
        attackTimer = attackCooldown;
        basicAttackTimer = basicAttackDuration;
        defendTimer = defendDuration;
    }

    void Update()
    {
        // Assign agent speed to Animator parameter
        animator.SetFloat("Speed", agent.velocity.magnitude);
    RotateShields();
    bool IsExistPlayer = CheckPlayerExist();

    if (IsExistPlayer)
    {
        switch (state)
        {
            case BossState.Idle:
                FindClosestTarget();
                
                if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
                {
                    state = BossState.BasicAttack;
                }
                break;

            case BossState.BasicAttack:
                PerformBasicAttack();
                break;

            case BossState.Defend:
                Defend();
                break;

            case BossState.Attacking:
                Attack();
                break;
        }

        if (state == BossState.Idle && target != null)
        {
            agent.SetDestination(target.position);
        }
    }  
    
    }

    void FindClosestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        GameObject closestPlayer = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            target = closestPlayer.transform;
        }
    }

    void PerformBasicAttack()
    {
      

        if (takingContinuousDamage)
        {
            state = BossState.Defend;
            defendTimer = defendDuration; // Reset defend duration
            return;
        }

        // Ensure that the boss only attacks if within the range
        if (target != null && Vector3.Distance(transform.position, target.position) > attackRange)
        {
            
            // Return early if the target is out of range
            state = BossState.Idle;
            animator.SetInteger("BasicAttack", 0);
            return;
        }

        if (basicAttackTimer <= 0)
        {
            state = BossState.Attacking;
            canSetPos = true;
            animator.SetInteger("BasicAttack", 0);
            attackTimer = attackCooldown; // Reset attack cooldown
        }
        else
        {
      
            
            basicAttackTimer -= Time.deltaTime;

            // Perform the basic attack animation and logic
            int randomAttack = Random.Range(1, maxBasicAttack+2);
            animator.SetInteger("BasicAttack", randomAttack);

            // Check if boss is taking continuous damage
            CheckContinuousDamage();
        }
    }

    void Defend()
    {
        wave.SetActive(true);
        waveEffect.SetActive(true);
       
        if (defendTimer <= 0)
        {
            waveScale = 1;
            waveEffectScale = 1;
            wave.transform.localScale = Vector3.one*3.5f;
            waveEffect.transform.localScale = Vector3.one;
            wave.SetActive(false);
            waveEffect.SetActive(false);
            state = BossState.BasicAttack;
            attackTimer = attackCooldown; // Attack süresini resetle
        }
        else
        {
            
            waveScale += Time.deltaTime * growthScaleWave;
            waveEffectScale += Time.deltaTime * growthScaleWaveEffect;
            wave.transform.localScale = (Vector3.one * waveScale)+Vector3.one*3.5f;
            waveEffect.transform.localScale = Vector3.one * waveEffectScale;
            defendTimer -= Time.deltaTime;

            // Burada savunma animasyonlarını ve işlemlerini yapabilirsin
            // Örneğin: animator.SetTrigger("Defend");
        }
    }

    void Attack()
    {
       
        if (attackTimer <= 0)
        {
         
            // Oyuncuların ortalama pozisyonunu bul
           

            // Geri çekilme yönünü hesapla
            if (canSetPos)
            {
                Vector3 averagePosition = FindAveragePlayerPosition();
                // Geri çekilme mesafesi
                float retreatDistance = 25f; // Burada geri çekilme mesafesini ayarlayabilirsiniz

                // Hedef geri çekilme pozisyonu
                retreatPos = new Vector3(averagePosition.x, transform.position.y,
                    transform.position.z + retreatDistance / Mathf.Clamp(transform.rotation.z-averagePosition.z,1,5));
                Debug.Log(retreatPos);
                // Geri çekilme pozisyonuna git
                agent.SetDestination(retreatPos);
                canSetPos = false;
            }
          
        Debug.Log((Vector3.Distance(transform.position, retreatPos)));

            // Geri çekilme işlemi tamamlandığında saldırıya geç
            if (Vector3.Distance(transform.position, retreatPos) < 5f)
            {
                int attackIndex = Random.Range(0, 2);
                PerformAttack(attackIndex);
                attackTimer = attackCooldown; // Saldırıdan sonra bekleme süresi
            }
        }
        else
        {
            attackTimer -= Time.deltaTime;
        }
    }

    Vector3 FindAveragePlayerPosition()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Vector3 sumPositions = Vector3.zero;

        foreach (GameObject player in players)
        {
            sumPositions += player.transform.position;
        }

        return sumPositions / players.Length; // Ortalama pozisyonu bul
    }


    void PerformAttack(int attackIndex)
    {
        switch (attackIndex)
        {
            case 0:
                secondAttack.StartCoroutine(secondAttack.PerformAreaAttack());
                break;
            case 1:
               
                thirdAttack.PerformDashAttack();
                break;
        }
        // Attack işlemi bittikten sonra tekrar Idle durumuna geç
        state = BossState.Idle;
        basicAttackTimer = basicAttackDuration; // Basic attack süresini resetle
    }

    void CheckContinuousDamage()
    {
        // Burada boss'un hasar aldığı zamanı kontrol et
        // Örneğin boss'un hasar aldığını bir event veya başka bir yolla algılayabilirsin

        if (takingContinuousDamage)
        {
            damageTakenTime += Time.deltaTime;

            if (damageTakenTime >= damageThreshold)
            {
                state = BossState.Defend;
                defendTimer = defendDuration; // Defend süresini resetle
            }
        }
        else
        {
            damageTakenTime = 0f; // Damage almıyorsa zamanlayıcıyı sıfırla
        }
    }

    public void OnDamageTaken()
    {
        takingContinuousDamage = true; // Hasar alındığında çağırılacak fonksiyon
    }

    public void OnDamageStopped()
    {
        takingContinuousDamage = false; // Hasar durduğunda çağırılacak fonksiyon
    }

    void RotateShields()
    {
        if(shield!=null)
            shield.Rotate(0,rotateSpeed*Time.deltaTime,0);
    }

    bool CheckPlayerExist()
    {
        RaycastHit[] hits;
        hits = Physics.BoxCastAll(origin.position, halfExtents, direction, Quaternion.identity, maxDistance, layerMask);

        // BoxCast'in temas ettiği tüm objeleri kontrol edin
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject != null&&hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color=Color.yellow;
        Gizmos.DrawWireCube(origin.position,halfExtents);
    }
}
