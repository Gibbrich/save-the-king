using System;
using Game.Scripts.ui;
using UnityEngine;

namespace Game.Scripts
{
    public class TutorialHandler : MonoBehaviour
    {
        public UiAnimatableElement element;
        public King king;
        public float tacticsCircleRadius = 2;
        public float battleCircleRadius = 1;
        [Tooltip("Degrees per second")]
        public float speed = 1;
        public Vector2 offset;
        public float angleToChangeVisibility = 45f;

        private Camera camera;
        private float sinToHide;

        private void Start()
        {
            camera = Camera.main;
            sinToHide = Mathf.Sin(angleToChangeVisibility * Mathf.Deg2Rad);
        }

        private void Update()
        {
            var point = camera.WorldToScreenPoint(king.transform.position);
            var value = Time.timeSinceLevelLoad * speed * Mathf.Deg2Rad;
            var sin = Mathf.Sin(value);
            var x = Mathf.Cos(value) * tacticsCircleRadius;
            var y = sin * tacticsCircleRadius;
            var pointX = point.x + x + offset.x;
            var pointY = point.y + y + offset.y;
            transform.position = new Vector3(pointX, pointY);

            if (sin >= sinToHide)
            {
                element.Hide(true);
            }
            else
            {
                element.Show(true);
            }
        }
    }
}