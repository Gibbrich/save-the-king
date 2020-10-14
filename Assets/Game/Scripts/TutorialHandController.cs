using System;
using Game.Scripts.ui;
using InControl;
using UnityEngine;

namespace Game.Scripts
{
    public class TutorialHandController : MonoBehaviour
    {
        public UiAnimatableElement element;
        public King king;
        public float tacticsCircleRadius = 2;
        public float battleCircleRadius = 1;
        [Tooltip("Degrees per second")]
        public float speed = 1;
        public Vector2 offset;
        public float angleToChangeVisibility = 45f;
        public float startPositionOffset = 3f;

        private Camera camera;
        private float sinToHide;
        private bool isBattle = false;
        private bool shouldAnimate = false;

        private void Start()
        {
            camera = Camera.main;
            sinToHide = Mathf.Sin(angleToChangeVisibility * Mathf.Deg2Rad);
        }

        private void Update()
        {
            if (shouldAnimate)
            {
                var radius = isBattle ? battleCircleRadius : tacticsCircleRadius;
                var point = camera.WorldToScreenPoint(king.transform.position);
                var value = (Time.timeSinceLevelLoad + startPositionOffset) * speed * Mathf.Deg2Rad;
                var sin = Mathf.Sin(value);
                var x = Mathf.Cos(value) * radius;
                var y = sin * radius;
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
                
                if (TouchManager.TouchCount > 0)
                {
                    Hide();            
                }
            }
        }

        public void Show(bool isBattle)
        {
            this.isBattle = isBattle;
            shouldAnimate = true;
        }

        public void Hide()
        {
            shouldAnimate = false;
            element.Hide(true);
        }
    }
}