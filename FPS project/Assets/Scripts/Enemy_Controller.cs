using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Controller : MonoBehaviour
{
    public float WalkSpeed;
    public float RunSpeed;
    public float MinDistanseToPlayer;
    public bool StillAlive;
    public Transform Player;
    bool CanEnemyMove;
    Rigidbody[] Bodies;
    Animator Anim;
    NavMeshAgent Agent;
    Enemy_Stats EStats;
    private void Start()
    {
        EStats = GetComponent<Enemy_Stats>();
        Anim = GetComponent<Animator>();
        Player = GameObject.Find("Player").transform;
        StillAlive = true;
        Bodies = GetComponentsInChildren<Rigidbody>();
        Agent = GetComponent<NavMeshAgent>();
        ResetRagdoll();
        CanEnemyMove = true;
    }
    private void Update()
    {
        if (CanEnemyMove)
        {
            EnemyStateMachine();
        }
    }
    void EnemyStateMachine()
    {
        float DistanseToPlayer = Vector3.Distance(transform.position, Player.position);
        if (!EStats.IsStanned)
        {
            if (DistanseToPlayer > MinDistanseToPlayer)
            {
                Agent.SetDestination(Player.position);
                Agent.speed = WalkSpeed;
                if (DistanseToPlayer > MinDistanseToPlayer * 2)
                {
                    Anim.SetInteger("State", 2);
                    Agent.speed = RunSpeed;
                }
                else
                    Anim.SetInteger("State", 1);
            }
            else
            {
                Anim.SetInteger("State", 0);
                Agent.speed = 0;
            }
        }
        else
        {
            Anim.SetInteger("State", 5);
            Agent.speed = 0;
        }
    }
    public void InGrab()
    {
        Anim.SetInteger("State", Random.Range(3, 5));

        CanEnemyMove = false;
    }
    public void Die()
    {
        StillAlive = false;
        Anim.enabled = false;
        Agent.enabled = false;
        ResetRagdoll();
    }
    void ResetRagdoll()
    {
        if (StillAlive)
            foreach (Rigidbody rb in Bodies)
                rb.isKinematic = true;
        else
        {
            foreach (Rigidbody rb in Bodies)
                rb.isKinematic = false;
        }
    }
}
