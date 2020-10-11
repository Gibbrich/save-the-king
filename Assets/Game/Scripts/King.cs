using System;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(OptimizedUnit))]
    public class King : MonoBehaviour
    {
        public OptimizedUnit unit;

        public event Action OnKingDeath = () => { };

        private void Start()
        {
            unit.Enable();
            unit.OnDeath = (_, __) => OnDeath();
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
        }
    }
}