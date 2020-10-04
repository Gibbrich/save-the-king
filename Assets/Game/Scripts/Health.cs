using UnityEngine;

namespace Game.Scripts
{
    public class Health : MonoBehaviour
    {
        public int maxHitPoints;
        public float CurrentHitPoints { get; set; }

        public bool IsDead() => CurrentHitPoints <= 0;
    }
}