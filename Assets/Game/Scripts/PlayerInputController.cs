using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class PlayerInputController : MonoBehaviour
    {
        public float minPointDistance;
        public float lineRenderHeight = 0.5f;

        private LineRenderer lineRenderer;
        private List<Vector3> points;
        private float minDistanceSquare;
        private Camera camera;
        private RaycastHit[] results = new RaycastHit[5];

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            points = new List<Vector3>();
            minDistanceSquare = Mathf.Pow(minPointDistance, 2);
            camera = Camera.main;
        }

        private void Update()
        {
            var touchCount = TouchManager.TouchCount;
            if (touchCount > 0)
            {
                var touch = TouchManager.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    points.Clear();
                }
                else
                {
                    AddPointIfNeed(touch.position);
                }
            }
        }

        private void AddPointIfNeed(Vector2 screenPoint)
        {
            var ray = camera.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y, camera.nearClipPlane));
            var hitsCount = Physics.RaycastNonAlloc(ray, results, 30f);
            if (hitsCount >= 1)
            {
                var worldPoint = results[0].point;
                worldPoint.y = lineRenderHeight;
                
                if (points.Count == 0)
                {
                    AddPoint(worldPoint);
                }
                else
                {
                    var lastPoint = points[points.Count - 1];
                    var sqrDistance = (worldPoint - lastPoint).sqrMagnitude;
                    if (sqrDistance >= minDistanceSquare)
                    {
                        AddPoint(worldPoint);
                    }
                }
            }
        }

        private void AddPoint(Vector3 worldPoint)
        {
            points.Add(worldPoint);
            var pointsCount = points.Count;
            lineRenderer.positionCount = pointsCount;
            lineRenderer.SetPosition(pointsCount - 1, worldPoint);
        }
    }
}