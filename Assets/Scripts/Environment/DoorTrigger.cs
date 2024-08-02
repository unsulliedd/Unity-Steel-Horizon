using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Transform Door;
    private bool canOpen;
    private Transform upPoint;
    private void Start()
    {
        upPoint = transform.GetChild(0).transform;

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
        if (canOpen && upPoint.position.y > Door.transform.position.y)
        {
            Door.Translate(0,Time.deltaTime*3,0);
        }
    }
}
