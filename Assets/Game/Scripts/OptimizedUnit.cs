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

        private Health currentTarget;

        private NavMeshAgent agent;
        private Collider collider;
        private Animator animator;
        private AudioSource source;

        private StateMachine<SoldierState> stateMachine;

        private float defaultStoppingDistance;

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

            //get default stopping distance
            defaultStoppingDistance = agent.stoppingDistance;

            collider = GetComponent<Collider>();
            isInitialized = true;

            health = GetComponent<Health>();
            
            stateMachine = new StateMachine<SoldierState>();
            stateMachine.AddState(SoldierState.IDLE, IdleStart, IdleUpdate, IdleStop);
            stateMachine.AddState(SoldierState.ATTACK, AttackStart, AttackUpdate, AttackStop);
            stateMachine.AddState(SoldierState.MOVE, MoveStart, MoveUpdate, MoveStop);
            stateMachine.CurrentState = SoldierState.IDLE;
        }

        private void IdleUpdate()
        {
            //find closest enemy
            currentTarget = FindTarget();
            
            if (currentTarget)
            {
                if (IsTargetWithinAttackRange(currentTarget.transform.position))
                {
                    stateMachine.CurrentState = SoldierState.ATTACK;
                }
                else if (shouldMoveToEnemy)
                {
                    stateMachine.CurrentState = SoldierState.MOVE;
                }
            }
        }

        [CanBeNull]
        private Health FindTarget()
        {
            //find closest enemy
            var potentialTargets = GameObject.FindGameObjectsWithTag(attackTag);
            if (currentTarget == null && potentialTargets.Length > 0)
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
            agent.stoppingDistance = defaultStoppingDistance;

            //move the agent around and set its destination to the enemy target
            agent.isStopped = false;
            var currentTargetPosition = currentTarget.gameObject.transform.position;
            agent.destination = currentTargetPosition;
            
            //play the running audio
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
            if (currentTarget.IsDead())
            {
                SwitchTarget();
            } 
            else if (IsTargetWithinAttackRange(currentTarget.transform.position))
            {
                stateMachine.CurrentState = SoldierState.ATTACK;
            }
        }

        private void AttackStart()
        {
            var currentTargetPosition = currentTarget.gameObject.transform.position;
            currentTargetPosition.y = transform.position.y;
            transform.LookAt(currentTargetPosition);
            animator.SetInteger(State, AttackState);

            //play the attack audio
            source.clip = attackAudio;
            source.Play();
        }

        private void SwitchTarget()
        {
            currentTarget = null;
            var target = FindTarget();
            if (target == null)
            {
                stateMachine.CurrentState = SoldierState.IDLE;
            }
            else
            {
                currentTarget = target;
                if (!IsTargetWithinAttackRange(target.transform.position))
                {
                    stateMachine.CurrentState = SoldierState.MOVE;
                }
            }
        }

        private void AttackUpdate()
        {
            if (currentTarget.IsDead())
            {
                SwitchTarget();
            } 
            else if (!IsTargetWithinAttackRange(currentTarget.gameObject.transform.position))
            {
                stateMachine.CurrentState = SoldierState.MOVE;
            }
            else
            {
                //apply damage to the enemy
                currentTarget.CurrentHitPoints -= Time.deltaTime * damage;
            }
        }

        private void AttackStop()
        {
            source.Stop();
        }

        private bool IsTargetWithinAttackRange(Vector3 target) => (target - transform.position).sqrMagnitude <= Mathf.Pow(attackRange, 2);

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
            source.Play();

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