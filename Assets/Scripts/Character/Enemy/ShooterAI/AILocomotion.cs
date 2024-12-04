using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AILocomotion : MonoBehaviour
{
    Animator animator;
    NavMeshAgent agent;
    AIAgent agentScript;
    
    // Start is called before the first frame update
    void Start()
    {
        agent=GetComponent<NavMeshAgent>();   
        animator=GetComponent<Animator>();
        agentScript=GetComponent<AIAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (agentScript.isdead)
        {
            agent.velocity = Vector3.zero;
            agent.angularSpeed = 0;
            agent.acceleration = 0;
            agent.speed = 0;
        }
        else
            animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
