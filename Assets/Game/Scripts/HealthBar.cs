using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts
{
    public class HealthBar : MonoBehaviour
    {
        public Slider healthBar;
        public Health health;
        public Vector3 offset = new Vector3(0, 2, 0);

        private void Start()
        {
            healthBar.maxValue = health.maxHitPoints;
        }

        private void Update()
        {
            transform.position = health.transform.position + offset;
            healthBar.value = health.CurrentHitPoints;
        }
    }
}