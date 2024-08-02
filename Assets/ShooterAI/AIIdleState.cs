using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIIdleState : AIState
{
    private int currentPatrolIndex = 0;

    public void Enter(AIAgent agent)
    {
        // Devriye başladığında en yakın noktaya git
        agent.navMeshAgent.SetDestination(agent.patrolPoints[currentPatrolIndex].position);
    }

    public void Exit(AIAgent agent)
    {
        // Çıkarken yapmanız gereken herhangi bir şey
    }

    public AIStateID GetID()
    {
        return AIStateID.Idle;
    }

    public void Update(AIAgent agent)
    {
        Vector3 playerDirection = agent.playerTransform.position - agent.transform.position;
        
        // Eğer oyuncu menzil dışındaysa
        if (playerDirection.magnitude > agent.config.maxSightDistance)
        {
            agent.weaponIK.canTurn = false;

            // Devriye hareketi
            Patrol(agent);
            return;
        }

        agent.weaponIK.canTurn = true;
        Vector3 agentDirection = agent.transform.forward;
        float dotProduct = Vector3.Dot(playerDirection, agentDirection);
        
        // Eğer oyuncu menzil içerisindeyse
        if (dotProduct > 0)
        {
            agent.StateMachine.ChangeState(AIStateID.ChasePlayer);
        }
    }

    private void Patrol(AIAgent agent)
    {
        if (!agent.navMeshAgent.pathPending && agent.navMeshAgent.remainingDistance <= agent.navMeshAgent.stoppingDistance)
        {
            // Sonraki devriye noktasına geç
            currentPatrolIndex = (currentPatrolIndex + 1) % agent.patrolPoints.Length;
            Debug.Log("Geçilen devriye noktası: " + currentPatrolIndex);
            agent.navMeshAgent.SetDestination(agent.patrolPoints[currentPatrolIndex].position);
        }
    }
}