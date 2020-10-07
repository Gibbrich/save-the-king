using System;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(OptimizedUnit))]
    public class King : MonoBehaviour
    {
        public OptimizedUnit unit;

        private void Start()
        {
            unit.Enable();
        }
    }
}