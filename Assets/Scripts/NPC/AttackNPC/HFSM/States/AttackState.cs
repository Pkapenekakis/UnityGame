using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityHFSM;

public class AttackState : EnemyStateBase
{
    private AnimatorStateInfo attackStateInfo;
    private bool isAnimationPlaying;
    public AttackState(bool needsExitTime, Enemy enemy, 
    Action<State<EnemyState, StateEvent>> onEnter, float exitTime = 0.3f) : base(needsExitTime, enemy, exitTime, onEnter){}

    public override void OnEnter()
    {
        agent.isStopped = true;
        base.OnEnter();
        animator.Play("Great Sword Slash");
        isAnimationPlaying = true;
        enemy.ResetAttackAnimationFlag();
    }

    public override void OnLogic()
    {
        // Check if the attack animation has completed and handle transitions
        if (isAnimationPlaying){  
            attackStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (attackStateInfo.IsName("Great Sword Slash") && attackStateInfo.normalizedTime >= 1f){ //normalizedTime represents the progress of the animation 0-1
                isAnimationPlaying = false;
                enemy.OnAttackAnimationComplete();
            }
        }
        base.OnLogic();
    }
}
