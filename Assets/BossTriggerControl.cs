using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTriggerControl : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            
        }
    }
}
