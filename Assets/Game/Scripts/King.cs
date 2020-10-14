using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    [RequireComponent(typeof(OptimizedUnit))]
    public class King : MonoBehaviour
    {
        public OptimizedUnit unit;
        public Slider healthBar;

        private Camera camera;
        private bool wasAttacked;

        public event Action OnKingDeath = () => { };
        public event Action OnKingAttacked = () => { };

        private void Start()
        {
            camera = Camera.main;
            unit.Enable();
            unit.OnDeath = (_, __) => OnDeath();
        }

        private void Update()
        {
            healthBar.transform.LookAt(2 * transform.position - camera.transform.position);
            var health = unit.health;
            healthBar.value = health.CurrentHitPoints;

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
            healthBar.maxValue = unit.health.maxHitPoints;
            healthBar.value = unit.health.CurrentHitPoints;
            wasAttacked = false;
        }
    }
}