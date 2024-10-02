using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : EnemyStateBase
{

    public IdleState(bool needsExitTime, Enemy enemy) : base(needsExitTime, enemy){
    }

    public override void OnEnter()
    {
        base.OnEnter();
        agent.isStopped = true;
        animator.Play("Idle"); 
        enemy.SetWaitTime(); 
    }

    public override void OnLogic(){
        enemy.UpdateWaitTime();
        base.OnLogic();
    }
}
