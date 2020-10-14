using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    [RequireComponent(typeof(OptimizedUnit))]
    public class King : MonoBehaviour
    {
        public HealthBar healthBar;
        public OptimizedUnit unit;

        private bool wasAttacked;

        public event Action OnKingDeath = () => { };
        public event Action OnKingAttacked = () => { };

        private void Start()
        {
            unit.Enable();
            unit.OnDeath = (_, __) => OnDeath();
        }

        private void Update()
        {
            var health = unit.health;

            if (!wasAttacked && !Mathf.Approximately(health.CurrentHitPoints, health.maxHitPoints))
            {
                wasAttacked = true;
                OnKingAttacked.Invoke();
            }
        }

        private void OnDeath()
        {
            OnKingDeath.Invoke();
            gameObject.SetActive(false);
            healthBar.gameObject.SetActive(false);
        }

        public void OnVictory()
        {
            unit.OnVictory();
        }

        public void Refresh(Vector3 kingPosition)
        {
            unit.Enable();
            gameObject.SetActive(true);
            transform.position = kingPosition;
            healthBar.gameObject.SetActive(true);
            wasAttacked = false;
        }
    }
}