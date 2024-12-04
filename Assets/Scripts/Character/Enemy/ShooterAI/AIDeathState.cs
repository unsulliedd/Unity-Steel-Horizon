using UnityEngine;

public class AIDeathState : AIState
{
    public void Enter(AIAgent agent)
    {
        agent.isdead = true;
        agent.animator.CrossFade("Death", 0.2f);
    }

    public void Exit(AIAgent agent)
    {
        
    }

    public AIStateID GetID()
    {
        return AIStateID.Death;
    }

    public void Update(AIAgent agent)
    {
        
    }
}
