using System;
using System.Collections;
using System.Collections.Generic;
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
        public List<GameObject> renderingObjects;
        public ParticleSystem deathEffect;
        public ParticleSystem victoryEffect;

        private Health attackTarget;
        private Vector3? navigationTarget;

        private AngerEmojiController angerEmojiController;
        private NavMeshAgent agent;
        private Collider collider;
        private Animator animator;
        private AudioSource source;
        private TargetSeeker targetSeeker;

        private StateMachine<SoldierState> stateMachine;

        private bool isInitialized;
        private bool isDeadTriggered;
        [CanBeNull] public Action<OptimizedUnit, bool> OnDeath = (unit, _) => { Destroy(unit.gameObject); };
        
        private static readonly int State = Animator.StringToHash("State");
        private static readonly int IdleState = 0;
        private static readonly int AttackState = 1;
        private static readonly int MoveState = 2;
        private static readonly int VictoryState = 4;


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

            targetSeeker = GetComponent<TargetSeeker>();
            angerEmojiController = GetComponentInChildren<AngerEmojiController>();
            
            stateMachine = new StateMachine<SoldierState>();
            stateMachine.AddState(SoldierState.IDLE, IdleStart, IdleUpdate, IdleStop);
            stateMachine.AddState(SoldierState.ATTACK, AttackStart, AttackUpdate, AttackStop);
            stateMachine.AddState(SoldierState.MOVE, MoveStart, MoveUpdate, MoveStop);
            stateMachine.CurrentState = SoldierState.IDLE;
        }

        public void OnVictory()
        {
            stateMachine.CurrentState = SoldierState.IDLE;
            animator.SetInteger(State, VictoryState);

            if (victoryEffect)
            {
                var victoryEffectMain = victoryEffect.main;
                victoryEffectMain.loop = true;
                victoryEffect.Play();
            }
        }

        public SoldierState GetState() => stateMachine.CurrentState;

        public void MoveToNavigationTarget(Vector3 navigationTarget)
        {
            attackTarget = null;
            this.navigationTarget = navigationTarget;
            stateMachine.CurrentState = SoldierState.MOVE;
        }

        private void MoveToAttackTarget()
        {
            this.navigationTarget = null;
            stateMachine.CurrentState = SoldierState.MOVE;
        }

        private bool IsAttackTargetAvailable() => attackTarget && !attackTarget.IsDead();

        private void IdleUpdate()
        {
            //find closest enemy
            attackTarget = FindAttackTarget();
            
            if (IsAttackTargetAvailable())
            {
                var targetPosition = attackTarget.transform.position;
                if (IsTargetWithinAttackRange(targetPosition))
                {
                    stateMachine.CurrentState = SoldierState.ATTACK;
                }
                else if (shouldMoveToEnemy)
                {
                    MoveToAttackTarget();
                }
            }
        }

        [CanBeNull]
        private Health FindAttackTarget()
        {
            var potentialTargets = GameObject.FindGameObjectsWithTag(attackTag);
            if (!IsAttackTargetAvailable() && potentialTargets.Length > 0)
            {
                return targetSeeker.GetTarget(potentialTargets);
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
            agent.stoppingDistance = IsAttackTargetAvailable() ? attackTargetStopDistance : navigationStoppingDistance;

            //move the agent around and set its destination to the enemy target
            agent.isStopped = false;
            agent.destination = IsAttackTargetAvailable() ? attackTarget.transform.position : navigationTarget.Value;
            
            //play the running audio
            source.Stop();
            source.clip = runAudio;
            source.Play();
            
            animator.SetInteger(State, MoveState);
        }

        private void MoveStop()
        {
            source.Stop();
            source.clip = null;
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
            } 
            else if (navigationTarget.HasValue)
            {
                if (IsTargetWithinStoppingDistance(navigationTarget.Value, navigationStoppingDistance + 0.2f))
                {
                    stateMachine.CurrentState = SoldierState.IDLE;
                }
            }
            else
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

            if (angerEmojiController)
            {
                angerEmojiController.CanSpawnEmoji = true;
            }
        }

        private void SwitchTarget()
        {
            attackTarget = null;
            var target = FindAttackTarget();
            if (target == null || target.IsDead())
            {
                stateMachine.CurrentState = SoldierState.IDLE;
            }
            else
            {
                var targetPosition = target.transform.position;
                if (IsTargetWithinAttackRange(targetPosition))
                {
                    attackTarget = target;
                    RotateToTarget();
                }
                else if (shouldMoveToEnemy)
                {
                    attackTarget = target;
                    MoveToAttackTarget();
                }
                else
                {
                    stateMachine.CurrentState = SoldierState.IDLE;
                }
            }
        }

        private void AttackUpdate()
        {
            if (!IsAttackTargetAvailable())
            {
                SwitchTarget();
            } 
            else if (!IsTargetWithinAttackRange(attackTarget.gameObject.transform.position) && shouldMoveToEnemy)
            {
                MoveToAttackTarget();
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
            source.clip = null;
            
            if (angerEmojiController)
            {
                angerEmojiController.CanSpawnEmoji = false;
            }
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
            if (!isInitialized)
            {
                Start();
            }
            
            isDeadTriggered = false;
            health.CurrentHitPoints = health.maxHitPoints;
            stateMachine.CurrentState = SoldierState.IDLE;
            //enable all the components
            agent.enabled = true;
            this.enabled = true;
            collider.enabled = true;
            
            for (int i = 0; i < renderingObjects.Count; i++)
            {
                renderingObjects[i].SetActive(true);
            }

            if (victoryEffect)
            {
                var victoryEffectMain = victoryEffect.main;
                victoryEffectMain.loop = false;
            }
        }

        public void SetVisibility(bool isVisible)
        {
            for (int i = 0; i < renderingObjects.Count; i++)
            {
                renderingObjects[i].SetActive(isVisible);
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
        }

        public IEnumerator die(bool shouldNotifyDeath = true)
        {
            stateMachine.CurrentState = SoldierState.IDLE;
            isDeadTriggered = true;
            deathEffect.Play();
            Disable();
            SetVisibility(false);
            yield return new WaitForSeconds(deathEffect.main.duration);
            OnDeath?.Invoke(this, shouldNotifyDeath);
        }
    }
}