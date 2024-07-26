using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : NetworkBehaviour
{
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Attacking
    }

    public EnemyState currentState;
    public float patrolDistance = 50f;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private bool movingToEndPoint = true;
    public float chaseRange = 20f;
    public float attackRange = 10f;
    private NavMeshAgent agent;
    public GameObject projectilePrefab;
    public Transform firePointLeft;
    public Transform firePointRight;
    public float fireRate = 1f;
    private float nextFireTime;
    public float rotationSpeed = 5f;
    public Transform headTransform;
    private Transform player;

    public float fieldOfViewAngle = 160f;

    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkedRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<Quaternion> networkedHeadRotation = new NetworkVariable<Quaternion>();

    public int maxAmmo = 30;
    private int currentAmmo;
    public float minReloadTime = 2f;
    public float maxReloadTime = 5f;
    private bool isReloading;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.Patrolling;
        currentAmmo = maxAmmo;
        isReloading = false;

        startPoint = transform.position;
        endPoint = transform.position + transform.forward * patrolDistance;

        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (IsServer)
        {
            switch (currentState)
            {
                case EnemyState.Patrolling:
                    Patrolling();
                    break;
                case EnemyState.Attacking:
                    Attacking();
                    break;
            }

            if ( currentState == EnemyState.Attacking)
            {
                LookAtPlayer();
            }

            networkedPosition.Value = transform.position;
            networkedRotation.Value = transform.rotation;
            networkedHeadRotation.Value = headTransform.rotation;
        }
        else
        {
            transform.position = networkedPosition.Value;
            transform.rotation = networkedRotation.Value;
            headTransform.rotation = networkedHeadRotation.Value;
        }
    }

    void Patrolling()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();

   
    }

    void GoToNextPatrolPoint()
    {
        if (movingToEndPoint)
        {
            agent.destination = endPoint;
        }
        else
        {
            agent.destination = startPoint;
        }

        movingToEndPoint = !movingToEndPoint;
    }


    void Attacking()
    {
        agent.isStopped = true;

        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;
            StartCoroutine(Fire());
        }
    }

    IEnumerator Fire()
    {
        if (currentAmmo > 0)
        {
            float randomDelay = Random.Range(0f, 1f);
            yield return new WaitForSeconds(randomDelay);

            if (player != null)
            {
                Debug.Log("Pozisyon1" + player.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).position + "Pozisyon2" + firePointRight.position);
                SpawnProjectileServerRpc(firePointLeft.position, firePointLeft.rotation, player.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).position);
                SpawnProjectileServerRpc(firePointRight.position, firePointRight.rotation, player.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).position);
            }
            currentAmmo--;
        }
        else
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        float reloadTime = Random.Range(minReloadTime, maxReloadTime);
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - headTransform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        headTransform.rotation = Quaternion.Slerp(headTransform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

   

    [ServerRpc]
    void SpawnProjectileServerRpc(Vector3 position, Quaternion rotation, Vector3 targetPosition)
    {
        GameObject projectile = Instantiate(projectilePrefab, position, rotation);
        projectile.GetComponent<NetworkObject>().Spawn();

        Vector3 direction = (targetPosition - position);
        var projectileBullet = projectile.GetComponent<ProjectileBullet>();

        if (projectileBullet != null)
        {
            projectileBullet.InitializeServerRpc(direction);
        }
        else
        {
            Debug.LogError("ProjectileBullet component could not be added to the projectilePrefab.");
        }
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        currentState = EnemyState.Attacking;
    }

    public void ClearPlayer()
    {
        player = null;
        currentState = EnemyState.Patrolling;
        agent.isStopped = false;
        GoToNextPatrolPoint();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Vector3 forward = transform.forward * chaseRange;
        Vector3 right = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * forward;
        Gizmos.DrawLine(transform.position, transform.position + right);
        Gizmos.DrawLine(transform.position, transform.position + left);
    }
}
