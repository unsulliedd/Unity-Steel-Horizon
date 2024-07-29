using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class GunController : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private float laserDuration = 0.05f;
    [SerializeField] private float laserRange = 600f;
    [SerializeField] private GameObject bulletPrefab; // Mermi prefab'ı referansı

   

    private void Start()
    {
   
    }

    public bool ShootGun()
    {
      //  if (!IsServer) return false; // Sadece sunucu işlemi gerçekleştirebilir

 

        if (Physics.Raycast(spawnPoint.position, transform.right, out RaycastHit hit, laserRange))
        {
            // Mermiyi yarat ve hedefe doğru gönder
            GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletController.SetTarget(hit.point);

            // Lazer çizgisini senkronize bir şekilde çiz
            DrawLaserClient(hit.point);

            if (hit.transform.gameObject.CompareTag("Player"))
            {
                Debug.Log("Oyuncu vuruldu");
                return true;
            }
        }
        else
        {
            // Lazer çizgisini hedef olmadan senkronize bir şekilde çiz
            DrawLaserClient(spawnPoint.position + transform.right * laserRange);
        }

        return false;
    }


    private void DrawLaserClient(Vector3 endPoint)
    {
       
        StartCoroutine(ShootLaser());
    }

    IEnumerator ShootLaser()
    {
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
    }

    private void Update()
    {
       

   
    }
}
