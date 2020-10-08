using UnityEngine;

namespace Game.Scripts
{
    public class SoldierTargetSeeker : MonoBehaviour, TargetSeeker
    {
        public Health GetTarget(GameObject[] potentialTargets)
        {
            //find all potential targets (enemies of this character)
            Health target = null;

            //if we want this character to communicate with his allies
            //if we're using the simple method:
            float closestDistance = Mathf.Infinity;

            foreach (GameObject potentialTarget in potentialTargets)
            {
                //check if there are enemies left to attack and check per enemy if its closest to this character
                var distance = (transform.position - potentialTarget.transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    var health = potentialTarget.GetComponent<Health>();
                    if (!health.IsDead())
                    {
                        //if this enemy is closest to character, set closest distance to distance between character and enemy
                        closestDistance = distance;
                        target = health;
                    }
                }
            }

            return target;
        }
    }
}