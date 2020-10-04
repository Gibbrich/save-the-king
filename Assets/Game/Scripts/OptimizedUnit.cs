using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Scripts
{
    [RequireComponent(typeof(Health))]
    public class OptimizedUnit : MonoBehaviour
    {
        public float damage;
        public string attackTag;
        public AudioClip attackAudio;
        public AudioClip runAudio;

        private Health currentTarget;
        private Health health;

        private NavMeshAgent agent;
        private Collider collider;
        private Animator animator;
        private AudioSource source;

        private float defaultStoppingDistance;

        private bool isInitialized;

        public bool isDead;
        [CanBeNull] public Action<OptimizedUnit> OnDeath = unit => { Destroy(unit.gameObject); };
        private static readonly int Attacking = Animator.StringToHash("Attacking");
        private static readonly int Start1 = Animator.StringToHash("Start");


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
        }

        void Update()
        {
            //find closest enemy
            if (currentTarget == null && GameObject.FindGameObjectsWithTag(attackTag).Length > 0)
            {
                currentTarget = findCurrentTarget();
            }

            //if character ran out of lives, it should die
            if (health.CurrentHitPoints < 1 && !isDead)
            {
                StartCoroutine(die());
            }

            //randomly walk across the battlefield if there's no targets left
            if (currentTarget == null)
            {
                Idle();
            }
            else
            {
                //if there are targets, make sure to use the default stopping distance
                agent.stoppingDistance = defaultStoppingDistance;

                //move the agent around and set its destination to the enemy target
                agent.isStopped = false;
                var currentTargetPosition = currentTarget.gameObject.transform.position;
                agent.destination = currentTargetPosition;

                //check if character has reached its target and than rotate towards target and attack it
                var distanceToTarget = Vector3.Distance(currentTargetPosition, transform.position);
                if (distanceToTarget <= agent.stoppingDistance)
                {
                    currentTargetPosition.y = transform.position.y;
                    transform.LookAt(currentTargetPosition);
                    animator.SetBool(Attacking, true);

                    //play the attack audio
                    if (source.clip != attackAudio)
                    {
                        source.clip = attackAudio;
                        source.Play();
                    }

                    //apply damage to the enemy
                    currentTarget.gameObject.GetComponent<Unit>().lives -= Time.deltaTime * damage;
                }

                //if its still traveling to the target, play running animation
                if (animator.GetBool(Attacking) && distanceToTarget > agent.stoppingDistance)
                {
                    animator.SetBool(Attacking, false);

                    //play the running audio
                    if (source.clip != runAudio)
                    {
                        source.clip = runAudio;
                        source.Play();
                    }
                }
            }
        }

        private void Idle()
        {
            // todo - implement
        }

        public void Enable()
        {
            isDead = false;
            currentTarget.CurrentHitPoints = currentTarget.maxHitPoints;
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

            animator.SetBool(Start1, true);
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

            //make sure it's playing an idle animation
            animator.SetBool(Start1, false);
        }

        public Health findCurrentTarget()
        {
            //find all potential targets (enemies of this character)
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(attackTag);
            Transform target = null;

            //if we want this character to communicate with his allies
            //if we're using the simple method:
            float closestDistance = Mathf.Infinity;

            foreach (GameObject potentialTarget in enemies)
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

        public IEnumerator die()
        {
            isDead = true;

            //wait a moment and destroy the original unit
            yield return new WaitForEndOfFrame();
            OnDeath?.Invoke(this);
        }
    }
}