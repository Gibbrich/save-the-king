using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class EnemyTargetSeeker : MonoBehaviour, TargetSeeker
    {
        public float toleranceRadius;

        private King king;
        private List<float> distances = new List<float>();

        private void Start()
        {
            king = FindObjectOfType<King>();
        }

        public Health GetTarget(GameObject[] potentialTargets)
        {
            if (potentialTargets.Length == 0)
            {
                return null;
            }
            
            distances.Clear();

            int kingPosition = -1;

            for (var i = 0; i < potentialTargets.Length; i++)
            {
                var potentialTarget = potentialTargets[i];
                //check if there are enemies left to attack and check per enemy if its closest to this character
                var distance = (transform.position - potentialTarget.transform.position).sqrMagnitude;
                distances.Add(distance);

                if (king && potentialTarget == king.gameObject)
                {
                    kingPosition = i;
                }
            }

            var minDistance = float.MaxValue;
            var targetId = -1;
            Health target = null;
            
            for (int i = 0; i < distances.Count; i++)
            {
                if (i == kingPosition)
                {
                    continue;
                }
                
                if (distances[i] < minDistance)
                {
                    var health = potentialTargets[i].GetComponent<Health>();
                    if (!health.IsDead())
                    {
                        //if this enemy is closest to character, set closest distance to distance between character and enemy
                        minDistance = distances[i];
                        targetId = i;
                        target = health;
                    }
                }
            }

            if (king && !king.unit.health.IsDead())
            {
                var distanceToKing = distances[kingPosition];
                var unitHealth = distanceToKing <= minDistance && minDistance > Mathf.Pow(toleranceRadius, 2) ? king.unit.health : target;
                
                return unitHealth;
            }
            else
            {
                return target;
            }
        }
    }
}