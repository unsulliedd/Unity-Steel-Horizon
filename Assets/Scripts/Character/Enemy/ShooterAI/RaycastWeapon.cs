using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastWeapon : MonoBehaviour
{
    public float range = 50f; // Raycast'in menzili
    public float fireRate = 1f; // Silahın ateş hızı
    public int damage = 10; // Verilen hasar
    public Transform firePoint; // Silahın ateş ettiği nokta
    public LayerMask targetMask;
    private CharacterNetworkManager _characterNetworkManager;
    [SerializeField] private GameObject bulletPrefab;// Hangi katmandaki objelere çarpacağını belirler

    private float nextTimeToFire = 0f;

    void Update()
    {
        if (Time.time >= nextTimeToFire)
        {
            if (CheckForTargets())
            {
                Shoot();
                nextTimeToFire = Time.time + 1f / fireRate;
            }
        }
    }

    bool CheckForTargets()
    {
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range, targetMask))
        {
            if (hit.collider.CompareTag("Player")) // Oyuncu etiketine sahip objeleri hedef alır
            {
                
               
                return true;
            }
        }
        return false;
    }

    void Shoot()
    {
        GameObject go=Instantiate(bulletPrefab, firePoint);
        go.GetComponent<Rigidbody>().AddForce(transform.forward*200,ForceMode.Acceleration);
        Destroy(go,3);
    }
}
