using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class ChaseState : EnemyStateBase
{
    private Transform target;
    public ChaseState(bool needsExitTime, Enemy enemy, Transform target) : base(needsExitTime, enemy){
        this.target = target;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        agent.enabled = true;
        agent.isStopped = false;
        animator.Play("WalkingForward");
    }

    public override void OnLogic()
    {
        base.OnLogic();
        if(!requestedExit){
            UnityEngine.Vector3 offsetPos = target.position + target.forward * 1.2f; //Stop a little bit forward of the player
            agent.SetDestination(offsetPos);
        }else if(agent.remainingDistance <= agent.stoppingDistance){
            //Continue moving to last known player location befor transitioning to idle
            fsm.StateCanExit();
        }
    }



}
