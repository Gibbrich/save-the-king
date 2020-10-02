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
        private List<Vector3> spawnPoints;
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
            spawnPoints = new List<Vector3>();
            minDistanceSquare = Mathf.Pow(minPointDistance, 2);
            spawnPointDistanceSquare = Mathf.Pow(spawnPointDistance, 2);
            camera = Camera.main;
            
            spawnPointsParent = new GameObject("spawnPointsParent");
            spawnPointsParent.transform.parent = transform;
            
            soldiersParent = new GameObject("soldiersParent");
            soldiersParent.transform.parent = transform;
            
            spawnPointPool = new Pool<GameObject>(50, CreateSpawnPoint, Destroy, WakeUpSpawnPoint, SetToSleepSpawnPoint);
            soldiersPool = new Pool<GameObject>(50, CreateSoldier, Destroy, WakeUpSoldier, SetAsleepSoldier);
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
                    spawnPointPool.ReleaseAll();
                    points.Clear();
                    spawnPoints.Clear();
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
        }

        private void AddPoint(Vector3 worldPoint)
        {
            points.Add(worldPoint);
            var pointsCount = points.Count;
            lineRenderer.positionCount = pointsCount;
            lineRenderer.SetPosition(pointsCount - 1, worldPoint);

            AddSpawnPointIfNeed(worldPoint);
        }

        private GameObject CreateSpawnPoint() => Instantiate(spawnPointPrefab, spawnPointsParent.transform);

        private void WakeUpSpawnPoint(GameObject spawnPoint)
        {
            spawnPoint.transform.position = spawnPoints[spawnPoints.Count - 1];
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

            for (int i = 0; i < spawnPoints.Count; i++)
            {
                var point = spawnPoints[i];
                var soldier = soldiersPool.GetNewObject();
                soldier.transform.position = point;
                soldier.transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.up);
            }

            levelManager.SpawnedSoldiers += spawnPoints.Count;
        }

        private void AddSpawnPointIfNeed(Vector3 linePosition)
        {
            if (spawnPoints.Count == 0)
            {
                spawnPoints.Add(linePosition);
                spawnPointPool.GetNewObject();
                levelManager.UpdateMaxAvailableSoldiersCount(spawnPoints.Count + 1);
            }
            else
            {
                var previousSpawnPoint = spawnPoints[spawnPoints.Count - 1];
                var distanceToPreviousSpawnPointSquare = (linePosition - previousSpawnPoint).sqrMagnitude;
                if (distanceToPreviousSpawnPointSquare >= spawnPointDistanceSquare)
                {
                    var distanceToNewSpawnPointSquare = distanceToPreviousSpawnPointSquare - spawnPointDistanceSquare;
                    var lerpCoefficient = 1 - distanceToNewSpawnPointSquare / distanceToPreviousSpawnPointSquare;
                    var newSpawnPoint = Vector3.Lerp(linePosition, previousSpawnPoint, lerpCoefficient);
                
                    spawnPoints.Add(newSpawnPoint);
                    spawnPointPool.GetNewObject();
                    levelManager.UpdateMaxAvailableSoldiersCount(spawnPoints.Count + 1);
                }
            }
        }
    }
}