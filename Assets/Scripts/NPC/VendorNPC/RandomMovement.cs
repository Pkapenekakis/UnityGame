using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI; //important

//if you use this code you are contractually obligated to like the YT video
public class RandomMovement : MonoBehaviour //don't forget to change the script name if you haven't
{
    public NavMeshAgent agent;
    public float range = 5; //radius of sphere
    public Animator animator;
    private GameObject player;
    
    float timer;
    float waitTimer = 3;
    float interactRange;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        StartCoroutine("TryFindPlayer");
        interactRange = PlayerInteract.interactRange + 0.2f; //small offset makes it work better
    }

    
    void Update(){
        if(player != null){ //when player respawns it becomes null so skip a few updates
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= interactRange){
                // Stop the agent if the player is within interaction distance
                agent.isStopped = true;
                animator.SetBool("isPatrolling", false);
            }else{   
                if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance){ //There is no path and We stopped
                    timer += Time.deltaTime; //when the NPC is stopped start updating the timer
                    animator.SetBool("isPatrolling", false);
                    if(timer >= waitTimer){
                        Patrol();
                        agent.isStopped = false;
                        animator.SetBool("isPatrolling", true);
                    }
                }else{
                    animator.SetBool("isPatrolling", true);
                }    
            }
        }
        
    }

    private void Patrol(){
        Vector3 point;
        if (RandomPoint(transform.position, range, out point)){
            agent.SetDestination(point);
            timer = 0;
            waitTimer = Random.Range(3.0f, 8.0f);
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

    IEnumerator TryFindPlayer(){
        while (player == null){
            try{
                player = GameObject.FindWithTag("Player");
            }catch{
            }
            if (player == null){
                yield return new WaitForSeconds(1);
            }
        }
    }

    
}