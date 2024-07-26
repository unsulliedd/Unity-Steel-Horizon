using System;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 20f;
    private Vector3 target;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetTarget(Vector3 target)
    {
        this.target = target;
        
        // Rigidbody'yi SetTarget fonksiyonunda da kontrol et
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        Vector3 direction = (target - transform.position);
        
        rb.velocity = direction * speed;
        Debug.Log(rb.velocity);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Çarpışma gerçekleşti");
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Duvara çarptı");
            Destroy(gameObject);
        }
    }
}