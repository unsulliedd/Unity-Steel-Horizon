using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAgent : MonoBehaviour
{
    // Start is called before the first frame update
    public AIStateMachine StateMachine;
    public NavMeshAgent navMeshAgent;
    public AIStateID initialState;
    public AIAgentConfig config;
    public Transform playerTransform;
    public WeaponIIIK weaponIK;
    public Transform[] patrolPoints;

    void Start()
    {
        Invoke(nameof(CheckPlayer),3);
        navMeshAgent = GetComponent<NavMeshAgent>();
        StateMachine=new AIStateMachine(this);
        StateMachine.RegisterState(new AIChasePlayer());
        StateMachine.RegisterState(new AIDeathState());
        StateMachine.RegisterState(new AIIdleState());
        StateMachine.ChangeState(initialState);
       
        
    }

    void CheckPlayer()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        StateMachine.Update();
    }
}
