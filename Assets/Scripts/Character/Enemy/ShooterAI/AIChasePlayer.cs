using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIChasePlayer : AIState
{
   
  
    float timer = 0.0f;
    public void Enter(AIAgent agent)
    {
        

    }

    public void Exit(AIAgent agent)
    {
       
    }

    public AIStateID GetID()
    {
        return AIStateID.ChasePlayer;
    }

    public void Update(AIAgent agent)
    {
       // Debug.Log(playerTransform.gameObject.name);
        Debug.Log(agent.gameObject.name);
        timer = Time.deltaTime;
        if (timer < 0)
        {
            float sqrDistance = (agent.playerTransform.position - agent.navMeshAgent.destination).sqrMagnitude;
            if (sqrDistance < Mathf.Sqrt(agent.config.maxDistance))
            {

                agent.navMeshAgent.SetDestination(agent.playerTransform.position);
            }
        }
        agent.navMeshAgent.SetDestination(agent.playerTransform.position);
        
    }
}
