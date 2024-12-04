using UnityEngine;

/// <summary>
/// Manages the behavior of a bullet, including its lifetime and collision handling.
/// </summary>
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f; // Lifetime of the bullet before it is disabled
    public float bulletSpeed = 1f;

    public Rigidbody rb; // Reference to the bullet's Rigidbody
    public AudioSource audioSource; // Reference to the bullet's AudioSource

    /// <summary>
    /// Called when the bullet is enabled. Initializes the Rigidbody and sets a timer to disable the bullet.
    /// </summary>
    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        rb.isKinematic = false; // Ensure the Rigidbody is not kinematic
        Invoke(nameof(Disable), lifeTime); // Schedule the bullet to be disabled after its lifetime
    }

    /// <summary>
    /// Called when the bullet is disabled. Cancels any pending Invokes.
    /// </summary>
    void OnDisable()
    {
        // Cancel any scheduled disables
        CancelInvoke(); 
    }

    /// <summary>
    /// Called when the bullet collides with another object.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // Perform a raycast to get detailed hit information
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
        {
            Vector3 hitPoint = hit.point; // Point of collision
            Vector3 hitNormal = hit.normal; // Normal at the point of collision
            string tag = collision.transform.tag;
            if (hit.transform.CompareTag("Boss"))
            {
                hit.transform.GetComponentInChildren<BossHealth>().TakeDamageToBoss(5);
            }
            else if (hit.transform.CompareTag("Enemy"))
            {
                hit.transform.GetComponentInChildren<EnemyHealth>().TakeDamage(25);
            }
            // Handle the collision impact based on the collided object's tag
            HandleCollisionImpact(tag, hitPoint, hitNormal);

            Debug.Log($"Hit: {collision.transform.name}");
        }

        // Disable the bullet after collision
        Disable();
    }

    /// <summary>
    /// Handles the impact of the bullet based on the collided object's tag.
    /// </summary>
    private void HandleCollisionImpact(string tag, Vector3 hitPoint, Vector3 hitNormal)
    {
        switch (tag)
        {
            case "Enemy":
                SpawnImpactEffect("BulletHit", hitPoint, hitNormal, 4);
                break;
            case "Boss":
                SpawnImpactEffect("BulletHit", hitPoint, hitNormal, 4);
                
                
                break;
            case "Player":
                // Spawn bullet hit effect for enemy or player
                SpawnImpactEffect("BulletHit", hitPoint, hitNormal, 4);
                break;
            default:
                // Spawn bullet hole and smoke effects for other surfaces
                SpawnImpactEffect("BulletHole", hitPoint, hitNormal, 25);
                SpawnImpactEffect("BulletHoleSmoke", hitPoint, hitNormal, 4);
                break;
        }
    }

    /// <summary>
    /// Spawns the impact effect from the object pool.
    /// </summary>
    private void SpawnImpactEffect(string tag, Vector3 hitPoint, Vector3 hitNormal, int disableDelay)
    {
        // Spawn the impact effect from the object pool
        GameObject impact = ObjectPoolManager.Instance.SpawnFromPool(tag, hitPoint, Quaternion.LookRotation(hitNormal));
        // Adjust the position and rotation of the impact effect
        impact.transform.SetPositionAndRotation(hitPoint + hitNormal * 0.001f, Quaternion.FromToRotation(Vector3.up, hitNormal));
        // Schedule the impact effect to be disabled after a delay
        ObjectPoolManager.Instance.DisableFromPool(tag, impact, disableDelay);
    }

    /// <summary>
    /// Disables the bullet and resets its Rigidbody's velocity.
    /// </summary>
    private void Disable()
    {
        // Deactivate the bullet
        gameObject.SetActive(false); 
        if (rb != null)
            rb.velocity = Vector3.zero;
    }
}