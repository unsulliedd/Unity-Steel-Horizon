using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAreaAttack : MonoBehaviour
{
    public GameObject areaAttackEffectPrefab; // Alan saldırısı efekt prefabı
    public float areaAttackInterval = 10.0f; // Alan saldırısı aralığı
    private float areaAttackTimer;
    public float sphereRadius = 5.0f; // Saldırı yarıçapı
    public LayerMask characterLayer;
    private GameObject[] bolts; // Karakterler için layer mask
    private int currentBoltIndex = 0;
    private Animator _animator;
    private void Start()
    {
        _animator = GetComponent<Animator>();
        areaAttackTimer = areaAttackInterval;
        bolts = new GameObject[4]; // 4 adet bolt için dizi oluştur
        for (int i = 0; i < bolts.Length; i++)
        {
           bolts[i] = Instantiate(areaAttackEffectPrefab);
           bolts[i].SetActive(false); // Başlangıçta tüm boltları deaktif et
        }
    }

  

  public  IEnumerator PerformAreaAttack()
    {
        // Boss'un pozisyonundan tüm karakterlere SphereCast yaparak saldırı
        _animator.SetBool("TriggerArea",true);
        yield return new WaitForSeconds(1f);
        _animator.SetBool("TriggerArea",false);
        Vector3 sphereCastStartPos = transform.position;
        RaycastHit[] hits = Physics.SphereCastAll(sphereCastStartPos, sphereRadius, Vector3.forward, 0f, characterLayer);

        foreach (RaycastHit hit in hits)
        {
            GameObject character = hit.collider.gameObject;

            // Alan saldırısı efekti oluştur
            Vector3 attackPosition = character.transform.position;
            GameObject bolt = bolts[currentBoltIndex];
            bolt.SetActive(true); // Boltu aktif hale getir
            bolt.transform.position = attackPosition;
            // Bir sonraki bolt için indeksi güncelle
            currentBoltIndex = (currentBoltIndex + 1) % bolts.Length;
        }

        yield return new WaitForSeconds(2);
        for (int i = 0; i < bolts.Length; i++)
        {
            bolts[i].SetActive(false);
        }
       
       
    }

    private void OnDrawGizmos()
    {
        // SphereCast alanını çizdir
        Gizmos.color = Color.red; // Alanın rengini ayarla
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}
