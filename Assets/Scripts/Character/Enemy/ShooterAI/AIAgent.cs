using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAgent : MonoBehaviour
{
    public AIStateMachine StateMachine;
    public NavMeshAgent navMeshAgent;
    public AIStateID initialState;
    public AIAgentConfig config;
    public Transform playerTransform;
    public WeaponIK weaponIK;
    public Transform[] patrolPoints;
    public Animator animator;
    public bool isdead;

    void Start()
    {
        Invoke(nameof(CheckPlayer),3);
        navMeshAgent = GetComponent<NavMeshAgent>();
        StateMachine=new AIStateMachine(this);
        StateMachine.RegisterState(new AIChasePlayer());
        StateMachine.RegisterState(new AIDeathState());
        StateMachine.RegisterState(new AIIdleState());
        StateMachine.ChangeState(initialState); 
        animator = GetComponent<Animator>();
    }

    void CheckPlayer()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (isdead)
            return;
        StateMachine.Update();
    }
}
