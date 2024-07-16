using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTriggerArea : MonoBehaviour
{
  
    [SerializeField] private WorldAIManager _worldAIManager;
    private GameObject[] tank;

    private void Start()
    {
        tank=GameObject.FindGameObjectsWithTag("Tank");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            foreach (var tankObject in tank)
            {
                tankObject.GetComponent<EnemyAI>().SetPlayer(other.transform);
            }
            Debug.Log("ALANA GİRDİ");
        }
    }
    

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var tankObject in tank)
            {
                tankObject.GetComponent<EnemyAI>().ClearPlayer();
            }
            
        }
    }
}
