
using System;
using UnityEngine;
using Unity.Netcode;

public class ProjectileBullet : NetworkBehaviour
{
    public float speed = 10f;
    private Rigidbody rb;
    private Vector3 direction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!IsServer) return;
        
        // Mermiyi yönlendirecek kuvveti uygula
        //rb.velocity = transform.forward * speed;
    }

    [ServerRpc]
    public void InitializeServerRpc(Vector3 target)
    {
        direction = target;
        Debug.Log("Değer "+direction);
    }

    private void FixedUpdate()
    {
        rb.velocity = direction* speed*Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Oyuncuya hasar ver veya gerekli işlemi yap
            Destroy(gameObject); // Mermiyi yok et
        }
    }
}
