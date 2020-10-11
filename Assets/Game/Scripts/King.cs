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

        public event Action OnKingDeath = () => { };

        private void Start()
        {
            unit.Enable();
            unit.OnDeath = (_, __) => OnDeath();
        }

        private void Update()
        {
            healthBar.transform.LookAt(2 * transform.position - Camera.main.transform.position);
            healthBar.value = unit.health.CurrentHitPoints;
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
        }
    }
}