using System;
using System.Collections;
using Gamelogic.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Scripts
{
    [RequireComponent(typeof(Health))]
    public class OptimizedUnit : MonoBehaviour
    {
        public float attackRange;
        public bool shouldMoveToEnemy;
        public float damage;
        public string attackTag;
        public AudioClip attackAudio;
        public AudioClip runAudio;
        public Health health;
        public float attackTargetStopDistance;
        public float navigationStoppingDistance = 0.1f;

        private Health attackTarget;
        private Vector3 navigationTarget;

        private NavMeshAgent agent;
        private Collider collider;
        private Animator animator;
        private AudioSource source;

        private StateMachine<SoldierState> stateMachine;

        private bool isInitialized;
        private bool isDeadTriggered;
        [CanBeNull] public Action<OptimizedUnit> OnDeath = unit => { Destroy(unit.gameObject); };
        
        private static readonly int State = Animator.StringToHash("State");
        private static readonly int IdleState = 0;
        private static readonly int AttackState = 1;
        private static readonly int MoveState = 2;


        void Start()
        {
            //get the audio source
            source = GetComponent<AudioSource>();

            //find navmesh agent component
            agent = gameObject.GetComponent<NavMeshAgent>();
            animator = gameObject.GetComponent<Animator>();

            collider = GetComponent<Collider>();
            isInitialized = true;

            health = GetComponent<Health>();
            
            stateMachine = new StateMachine<SoldierState>();
            stateMachine.AddState(SoldierState.IDLE, IdleStart, IdleUpdate, IdleStop);
            stateMachine.AddState(SoldierState.ATTACK, AttackStart, AttackUpdate, AttackStop);
            stateMachine.AddState(SoldierState.MOVE, MoveStart, MoveUpdate, MoveStop);
            stateMachine.CurrentState = SoldierState.IDLE;
        }

        public SoldierState GetState() => stateMachine.CurrentState;

        public void MoveToTarget(Vector3 navigationTarget, bool shouldResetAttackTarget = false)
        {
            if (shouldResetAttackTarget)
            {
                attackTarget = null;
            }

            this.navigationTarget = navigationTarget;
            stateMachine.CurrentState = SoldierState.MOVE;
        }

        private void IdleUpdate()
        {
            //find closest enemy
            attackTarget = FindAttackTarget();
            
            if (attackTarget)
            {
                var targetPosition = attackTarget.transform.position;
                if (IsTargetWithinAttackRange(targetPosition))
                {
                    stateMachine.CurrentState = SoldierState.ATTACK;
                }
                else if (shouldMoveToEnemy)
                {
                    MoveToTarget(targetPosition);
                }
            }
        }

        [CanBeNull]
        private Health FindAttackTarget()
        {
            //find closest enemy
            var potentialTargets = GameObject.FindGameObjectsWithTag(attackTag);
            if (attackTarget == null && potentialTargets.Length > 0)
            {
                //find all potential targets (enemies of this character)
                Transform target = null;

                //if we want this character to communicate with his allies
                //if we're using the simple method:
                float closestDistance = Mathf.Infinity;

                foreach (GameObject potentialTarget in potentialTargets)
                {
                    //check if there are enemies left to attack and check per enemy if its closest to this character
                    var distance = (transform.position - potentialTarget.transform.position).sqrMagnitude;
                    if (distance < closestDistance)
                    {
                        //if this enemy is closest to character, set closest distance to distance between character and enemy
                        closestDistance = distance;
                        target = potentialTarget.transform;
                    }
                }

                return target != null ? target.GetComponent<Health>() : null;
            }
            else
            {
                return null;
            }
        }

        private void IdleStart()
        {
            animator.SetInteger(State, IdleState);
        }

        private void IdleStop()
        {
            
        }

        private void MoveStart()
        {
            //if there are targets, make sure to use the default stopping distance
            agent.stoppingDistance = attackTarget ? attackTargetStopDistance : navigationStoppingDistance;

            //move the agent around and set its destination to the enemy target
            agent.isStopped = false;
            agent.destination = navigationTarget;
            
            //play the running audio
            source.Stop();
            source.clip = runAudio;
            source.Play();
            
            animator.SetInteger(State, MoveState);
        }

        private void MoveStop()
        {
            source.Stop();
        }

        private void MoveUpdate()
        {
            if (attackTarget)
            {
                if (attackTarget.IsDead())
                {
                    SwitchTarget();
                } 
                else if (IsTargetWithinAttackRange(attackTarget.transform.position))
                {
                    stateMachine.CurrentState = SoldierState.ATTACK;
                }
                else if (shouldMoveToEnemy)
                {
                    agent.SetDestination(attackTarget.transform.position);
                }
            } else if (IsTargetWithinStoppingDistance(navigationTarget, navigationStoppingDistance + 0.2f))
            {
                stateMachine.CurrentState = SoldierState.IDLE;
            }
        }

        private void RotateToTarget()
        {
            var currentTargetPosition = attackTarget.gameObject.transform.position;
            currentTargetPosition.y = transform.position.y;
            transform.LookAt(currentTargetPosition);
        }

        private void AttackStart()
        {
            RotateToTarget();
            animator.SetInteger(State, AttackState);

            //play the attack audio
            source.Stop();
            source.clip = attackAudio;
            source.Play();
        }

        private void SwitchTarget()
        {
            attackTarget = null;
            var target = FindAttackTarget();
            if (target == null)
            {
                stateMachine.CurrentState = SoldierState.IDLE;
            }
            else
            {
                attackTarget = target;
                var targetPosition = target.transform.position;
                if (!IsTargetWithinAttackRange(targetPosition))
                {
                    MoveToTarget(targetPosition);
                }
                else
                {
                    RotateToTarget();
                }
            }
        }

        private void AttackUpdate()
        {
            if (attackTarget == null || attackTarget.IsDead())
            {
                SwitchTarget();
            } 
            else if (!IsTargetWithinAttackRange(attackTarget.gameObject.transform.position))
            {
                MoveToTarget(attackTarget.gameObject.transform.position);
            }
            else
            {
                //apply damage to the enemy
                attackTarget.CurrentHitPoints -= Time.deltaTime * damage;
            }
        }

        private void AttackStop()
        {
            source.Stop();
        }

        private bool IsTargetWithinAttackRange(Vector3 target) => (target - transform.position).sqrMagnitude <= Mathf.Pow(attackRange, 2);

        private bool IsTargetWithinStoppingDistance(Vector3 target, float stoppingDistance) =>
            (target - transform.position).sqrMagnitude <= Mathf.Pow(stoppingDistance, 2);

        void Update()
        {
            if (health.IsDead())
            {
                if (!isDeadTriggered)
                {
                    StartCoroutine(die());
                }
            }
            else
            {
                stateMachine.Update();
            }
        }

        public void Enable()
        {
            isDeadTriggered = false;
            health.CurrentHitPoints = health.maxHitPoints;
            //enable all the components
            agent.enabled = true;
            this.enabled = true;
            collider.enabled = true;

            //show particles
            foreach (ParticleSystem particles in GetComponentsInChildren<ParticleSystem>())
            {
                particles.gameObject.SetActive(true);
            }
        }

        public void Disable()
        {
            if (!isInitialized)
            {
                Start();
            }

            //disable the navmesh agent component
            agent.enabled = false;

            //disable the unit script
            enabled = false;

            //disable the collider
            collider.enabled = false;

            //disable any particles
            foreach (ParticleSystem particles in GetComponentsInChildren<ParticleSystem>())
            {
                particles.gameObject.SetActive(false);
            }
        }

        public IEnumerator die()
        {
            isDeadTriggered = true;

            //wait a moment and destroy the original unit
            yield return new WaitForEndOfFrame();
            OnDeath?.Invoke(this);
        }
    }
}