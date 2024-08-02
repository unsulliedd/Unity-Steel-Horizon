using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AILocomotion : MonoBehaviour
{

    Animator animator;
    NavMeshAgent agent;
    
    // Start is called before the first frame update
    void Start()
    {
     agent=GetComponent<NavMeshAgent>();   
        animator=GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
  
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
