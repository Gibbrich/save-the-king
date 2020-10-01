using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using InControl;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class PlayerSoldierSpawnManager : MonoBehaviour
    {
        public LevelManager levelManager;
        public GameObject king;
        public GameObject spawnPointPrefab;
        public GameObject soldierPrefab;
        public float minPointDistance;
        public float lineRenderHeight = 0.5f;
        public float spawnPointDistance = 0.5f;
        public float soldierRotationOffset = 90f;

        private LineRenderer lineRenderer;
        private List<Vector3> points;
        private List<int> spawnPointsIndices;
        private float minDistanceSquare;
        private float spawnPointDistanceSquare;
        private Camera camera;
        private RaycastHit[] results = new RaycastHit[1];
        private Pool<GameObject> spawnPointPool;
        private Pool<GameObject> soldiersPool;
        private GameObject spawnPointsParent;
        private GameObject soldiersParent;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            points = new List<Vector3>();
            spawnPointsIndices = new List<int>();
            minDistanceSquare = Mathf.Pow(minPointDistance, 2);
            spawnPointDistanceSquare = Mathf.Pow(spawnPointDistance, 2);
            camera = Camera.main;
            
            spawnPointsParent = new GameObject("spawnPointsParent");
            spawnPointsParent.transform.parent = transform;
            
            soldiersParent = new GameObject("soldiersParent");
            soldiersParent.transform.parent = transform;
            
            spawnPointPool = new Pool<GameObject>(50, CreateSpawnPoint, Destroy, WakeUpSpawnPoint, SetToSleepSpawnPoint);
            soldiersPool = new Pool<GameObject>(levelManager.soldiersCount, CreateSoldier, Destroy, WakeUpSoldier, SetAsleepSoldier);
        }

        private void Update()
        {
            var touchCount = TouchManager.TouchCount;
            if (touchCount > 0)
            {
                var touch = TouchManager.GetTouch(0);
                if (touch.phase == TouchPhase.Ended)
                {
                    SpawnSoldiers();
                    spawnPointPool.SetAllItemsToSleep();
                    points.Clear();
                    spawnPointsIndices.Clear();
                    lineRenderer.positionCount = 0;
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
                var raycastHit = results[0];

                if (results[0].transform.CompareTag("Floor"))
                {
                    var worldPoint = raycastHit.point;
                    worldPoint.y = lineRenderHeight;
                
                    if (points.Count == 0)
                    {
                        AddPoint(worldPoint, true);
                    }
                    else
                    {
                        var lastPoint = points[points.Count - 1];
                        var sqrDistance = (worldPoint - lastPoint).sqrMagnitude;
                        if (sqrDistance >= minDistanceSquare)
                        {
                            var previousSpawnPointIndex = spawnPointsIndices[spawnPointsIndices.Count - 1];
                            var shouldAddSpawnPoint = (worldPoint - points[previousSpawnPointIndex]).sqrMagnitude >= spawnPointDistanceSquare;
                            AddPoint(worldPoint, shouldAddSpawnPoint);
                        }
                    }
                }
            }
        }

        private void AddPoint(Vector3 worldPoint, bool shouldAddSpawnPoint)
        {
            points.Add(worldPoint);
            var pointsCount = points.Count;
            lineRenderer.positionCount = pointsCount;
            lineRenderer.SetPosition(pointsCount - 1, worldPoint);
            
            if (shouldAddSpawnPoint)
            {
                spawnPointsIndices.Add(points.Count - 1);
                spawnPointPool.GetNewObject();
            }
        }

        private GameObject CreateSpawnPoint() => Instantiate(spawnPointPrefab, spawnPointsParent.transform);

        private void WakeUpSpawnPoint(GameObject spawnPoint)
        {
            var spawnPointsIndex = spawnPointsIndices[spawnPointsIndices.Count - 1];
            spawnPoint.transform.position = points[spawnPointsIndex];
            spawnPoint.SetActive(true);
        }

        private void SetToSleepSpawnPoint(GameObject spawnPoint) => spawnPoint.SetActive(false);

        private GameObject CreateSoldier() => Instantiate(soldierPrefab, soldiersParent.transform);

        private void WakeUpSoldier(GameObject soldier)
        {
            soldier.SetActive(true);
        }

        private void SetAsleepSoldier(GameObject soldier)
        {
            soldier.SetActive(false);
        }

        private void SpawnSoldiers()
        {
            var midPointId = points.Count / 2;
            var midPoint = points[midPointId];
            var midPoint2d = new Vector2(midPoint.x, midPoint.z);
            
            var kingPosition = king.transform.position;
            var kingPosition2d = new Vector2(kingPosition.x, kingPosition.z);
            
            var direction = midPoint2d - kingPosition2d;
            var angleDeg = soldierRotationOffset - Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            for (int i = 0; i < spawnPointsIndices.Count; i++)
            {
                var pointId = spawnPointsIndices[i];
                var point = points[pointId];
                var soldier = soldiersPool.GetNewObject();
                soldier.transform.position = point;
                soldier.transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.up);
            }
        }
    }
}