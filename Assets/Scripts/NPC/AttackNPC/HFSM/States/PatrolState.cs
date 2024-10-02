using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : EnemyStateBase
{

    private NavMeshAgent enemyAgent;
    private Animator enemyAnimator;
    //private GameObject player;
    private float range = 16f; // You can adjust this value or pass it as a parameter if needed
    private bool firstLoop = true;
    public PatrolState(bool needsExitTime, Enemy enemy) : base(needsExitTime, enemy){
        enemyAgent = enemy.GetComponent<NavMeshAgent>();
        enemyAnimator = enemy.GetComponent<Animator>();
        //player = GameObject.FindWithTag("Player");
        //interactRange = PlayerInteract.interactRange + 0.2f; // Adjust as necessary
    }

    public override void OnEnter()
    {
        base.OnEnter();
        firstLoop = true;
        enemy.patrolFinished = false;
        enemyAgent.enabled = true;
        enemyAgent.isStopped = false;
        enemyAnimator.Play("WalkingForward");
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if(firstLoop){
            Patrol();
        }else{
            if (enemyAgent.remainingDistance <= enemyAgent.stoppingDistance){
                enemy.patrolFinished = true;  
            }
        }

        
        
        
    }

    /*
    private void Patrol(){
        if (enemyAgent.remainingDistance <= enemyAgent.stoppingDistance){
            Vector3 point;
            if (RandomPoint(enemy.transform.position, range, out point)){
                    enemyAgent.SetDestination(point);
            }
        }
    } */

    private void Patrol(){
        Vector3 point;
        if (RandomPoint(enemy.transform.position, range, out point)){
                enemyAgent.SetDestination(point);
                firstLoop = false;
        } 
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result){

        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)){ 
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}
