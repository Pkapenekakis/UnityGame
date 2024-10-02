using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState : EnemyStateBase
{
    public DieState(bool needsExitTime, Enemy enemy) : base(needsExitTime, enemy){

    }

    public override void OnEnter()
    {
        base.OnEnter();
        agent.isStopped = true;
        animator.Play("Death");
        enemy.StartCoroutine("WaitAndDestroy");
    }

    public override void OnLogic(){
        base.OnLogic();
    }
}
