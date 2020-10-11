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
            unit.OnDeath = _ => OnDeath();
        }

        private void OnDeath()
        {
            OnKingDeath.Invoke();
            Destroy(gameObject);
        }

        public void OnVictory()
        {
            unit.OnVictory();
        }

        public void Refresh()
        {
            // refresh King hp
            unit.Enable();
            // todo - set king position to default one
        }
    }
}