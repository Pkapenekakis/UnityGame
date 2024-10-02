using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    private StateMachine<EnemyState, StateEvent> enemyFSM;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private GameObject player;

    public EnemyStats stats;

    [Header("Sensors")]
    [SerializeField] private PlayerSensor followPlayerSensor;
    [SerializeField] private PlayerSensor meleeAttackSensor;

    [Header("Attack Config")]
    [SerializeField]
    [Range(0.1f, 5f)]
    private float AttackCooldown = 2;

    public bool patrolFinished = false;
    private float waitTime = 0;
    private float timer = 0;
    private bool isFSMSetup = false;

    public bool attackAnimationCompleted = false;

    [Space]
        [Header("Debug Info")]
        [SerializeField]
        private bool isInMeleeRange;
        [SerializeField]
        private bool isInChaseRange;
        [SerializeField]
        private float lastAttackTime;
        [SerializeField]

    private void Start() {
        StartCoroutine("TryFindPlayer");
        followPlayerSensor.OnPlayerEnter += followPlayerSensor_OnPlayerEnter;
        followPlayerSensor.OnPlayerExit += followPlayerSensor_OnPlayerExit;
        meleeAttackSensor.OnPlayerEnter += meleeAttackSensor_OnPlayerEnter;
        meleeAttackSensor.OnPlayerExit += meleeAttackSensor_OnPlayerExit;      
    }

    

    void Awake(){
        navMeshAgent = GetComponent<NavMeshAgent>();
        stats = GetComponent<EnemyStats>();
        navMeshAgent.stoppingDistance = 1.0f;
        navMeshAgent.speed = stats.moveSpeed;
        animator = GetComponent<Animator>();
        enemyFSM = new(); //new StateMachine<EnemyState, StateEvent>();

        
    }

    private void Update() {
        if(isFSMSetup){
            enemyFSM.OnLogic();
        }
        
    }

    private void OnAttack(State<EnemyState, StateEvent> state){
        transform.LookAt(player.transform.position);
        lastAttackTime = Time.time;
    }

    private void followPlayerSensor_OnPlayerExit(Vector3 lastKnownPos)
    {
        enemyFSM.Trigger(StateEvent.LostPlayer);
        isInChaseRange = false;
    }

    private void followPlayerSensor_OnPlayerEnter(Transform player)
    {
        enemyFSM.Trigger(StateEvent.DetectPlayer);
        isInChaseRange = true;
    }

    private void meleeAttackSensor_OnPlayerExit(Vector3 lastKnownPos)
    {
        isInMeleeRange = false;
    }

    private void meleeAttackSensor_OnPlayerEnter(Transform player)
    {
        isInMeleeRange = true;
    }

    public void SetWaitTime()
    {
        waitTime = UnityEngine.Random.Range(3.0f, 8.0f);
        timer = 0;
    }

    public void UpdateWaitTime()
    {
        timer += Time.deltaTime;
    }

    public void OnAttackAnimationComplete()
    {
        attackAnimationCompleted = true;
    }

    public void ResetAttackAnimationFlag()
    {
        attackAnimationCompleted = false;
    }

    private bool ShouldMelee(Transition<EnemyState> Transition) => lastAttackTime + AttackCooldown <= Time.time && isInMeleeRange;
    private bool IsWithinIdleRange(Transition<EnemyState> Transition) => navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    private bool IsNotWithinIdleRange(Transition<EnemyState> Transition) => !IsWithinIdleRange(Transition);    

    IEnumerator TryFindPlayer(){
        while (player == null){
            try{
                player = GameObject.FindWithTag("Player");
                enemyFSM.AddState(EnemyState.Chase, new ChaseState(true,this,player.transform));
                enemyFSM.AddState(EnemyState.Attack, new AttackState(true,this,OnAttack));
                //add states
                enemyFSM.AddState(EnemyState.Idle, new IdleState(false,this));
                enemyFSM.AddState(EnemyState.Die, new DieState(false,this));
                enemyFSM.AddState(EnemyState.Patrol, new PatrolState(true,this));
        

                //add transitions

                //From Idle Transisions

                    //idle --> patrol
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Idle,EnemyState.Patrol, transition => timer >= waitTime)); 
                    //idle --> chase
                enemyFSM.AddTriggerTransition(StateEvent.DetectPlayer, new Transition<EnemyState>(EnemyState.Idle, EnemyState.Chase, forceInstantly: true));
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Idle, EnemyState.Chase, transition 
                    => isInChaseRange && (Vector3.Distance(player.transform.position, transform.position) > navMeshAgent.stoppingDistance) ));
                    //idle --> attack    
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Idle,EnemyState.Attack, ShouldMelee, forceInstantly: true));
   
                
                

                //From Patrol Transitions
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Patrol, EnemyState.Idle, transition => patrolFinished));
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Patrol,EnemyState.Attack, ShouldMelee, forceInstantly: true));
                enemyFSM.AddTriggerTransition(StateEvent.DetectPlayer, new Transition<EnemyState>(EnemyState.Patrol, EnemyState.Chase, forceInstantly: true));
                
                //From Chase Transitions
                enemyFSM.AddTriggerTransition(StateEvent.LostPlayer, new Transition<EnemyState>(EnemyState.Chase, EnemyState.Idle));
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Chase, EnemyState.Idle, transition
                    => !isInChaseRange && (Vector3.Distance(player.transform.position, transform.position) <= navMeshAgent.stoppingDistance) ));
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Chase,EnemyState.Attack, ShouldMelee, forceInstantly: true));

                //From Attack Transitions
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Attack,EnemyState.Chase ,transition => IsNotWithinIdleRange(transition) && attackAnimationCompleted));
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Attack,EnemyState.Idle, transition => IsWithinIdleRange(transition) && attackAnimationCompleted));
                
                

                //From Die transitions??
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Idle, EnemyState.Die, transition => stats.isDead == true, forceInstantly: true));
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Patrol, EnemyState.Die, transition => stats.isDead == true, forceInstantly: true));
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Chase, EnemyState.Die, transition => stats.isDead == true, forceInstantly: true));
                enemyFSM.AddTransition(new Transition<EnemyState>(EnemyState.Attack, EnemyState.Die, transition => stats.isDead == true, forceInstantly: true));

                enemyFSM.SetStartState(EnemyState.Idle);
                enemyFSM.Init();
                isFSMSetup = true;
            }catch{
            }
            if (player == null){
                yield return new WaitForSeconds(1);
            }
        }
    }

    public IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(30f);
        UnityEngine.Object.Destroy(gameObject);
    }
}
