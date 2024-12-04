using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    private bool canOpen;
    private Transform upPoint;
    private BoxCollider boxCollider;

    private void Start()
    {
        upPoint = transform.GetChild(0).transform;
        boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.gameObject.CompareTag("Player"))
        {
            canOpen = true;
        }
        
    }

    private void Update()
    {
        if (canOpen && upPoint.position.y > transform.position.y)
        {
            transform.Translate(0,Time.deltaTime*3,0);
        }
    }
}
