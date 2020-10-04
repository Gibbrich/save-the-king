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
        public Unit soldierPrefab;
        public float minPointDistance;
        public float lineRenderHeight = 0.5f;
        public float spawnPointDistance = 0.5f;
        public float soldierRotationOffset = 90f;

        private LineRenderer lineRenderer;
        private List<Vector3> points;
        private float minDistanceSquare;
        private float spawnPointDistanceSquare;
        private Camera camera;
        private RaycastHit[] results = new RaycastHit[1];
        private Pool<GameObject> spawnPointPool;
        private Pool<Unit> soldiersPool;
        private GameObject spawnPointsParent;
        private GameObject soldiersParent;
        private SpawnPointsHolder.SpawnPointsHolder spawnPointsHolder;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            points = new List<Vector3>();
            spawnPointsHolder = new SpawnPointsHolder.SpawnPointsHolder(spawnPointDistance);
            minDistanceSquare = Mathf.Pow(minPointDistance, 2);
            camera = Camera.main;
            
            spawnPointsParent = new GameObject("spawnPointsParent");
            spawnPointsParent.transform.parent = transform;
            
            soldiersParent = new GameObject("soldiersParent");
            soldiersParent.transform.parent = transform;
            
            spawnPointPool = new Pool<GameObject>(50, CreateSpawnPoint, Destroy, WakeUpSpawnPoint, SetToSleepSpawnPoint);
            soldiersPool = new Pool<Unit>(50, CreateSoldier, DestroySoldier, WakeUpSoldier, SetAsleepSoldier);
        }

        private void Update()
        {
            var touchCount = TouchManager.TouchCount;
            if (touchCount > 0)
            {
                var touch = TouchManager.GetTouch(0);
                if (touch.phase == TouchPhase.Ended)
                {
                    if (spawnPointsHolder.SpawnPoints.Count > 0)
                    {
                        SpawnSoldiers();
                        spawnPointPool.ReleaseAll();
                        points.Clear();
                        spawnPointsHolder.SpawnPoints.Clear();
                        lineRenderer.positionCount = 0;
                    }
                }
                else
                {
                    AddPointIfNeed(touch.position);
                }
            }
        }

        public void OnBattleStart()
        {
            var soldiers = soldiersPool.GetActiveObjects();
            for (var i = 0; i < soldiers.Count; i++)
            {
                soldiers[i].Enable();
            }
        }

        private void AddPointIfNeed(Vector2 screenPoint)
        {
            if (spawnPointsHolder.SpawnPoints.Count >= levelManager.maxAvailableSoldiers - levelManager.SpawnedSoldiers)
            {
                return;
            }
            
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
            spawnPoint.SetActive(true);
        }

        private void SetToSleepSpawnPoint(GameObject spawnPoint) => spawnPoint.SetActive(false);

        private void DestroySoldier(Unit soldier)
        {
            soldier.OnDeath = null;
            Destroy(soldier.gameObject);
        }

        private Unit CreateSoldier()
        {
            var unit = Instantiate(soldierPrefab, soldiersParent.transform);
            unit.Disable();
            unit.OnDeath = SetAsleepSoldier;
            return unit;
        }

        private void WakeUpSoldier(Unit soldier)
        {
            soldier.gameObject.SetActive(true);
        }

        private void SetAsleepSoldier(Unit soldier)
        {
            soldier.Disable();
            soldier.gameObject.SetActive(false);
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

            for (int i = 0; i < spawnPointsHolder.SpawnPoints.Count; i++)
            {
                var point = spawnPointsHolder.SpawnPoints[i];
                var soldier = soldiersPool.GetNewObject();
                soldier.transform.position = point;
                soldier.transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.up);
            }

            levelManager.UpdateSpawnedSoldiers(spawnPointsHolder.SpawnPoints.Count);
        }

        private void AddSpawnPointIfNeed(Vector3 linePosition)
        {
            var result = spawnPointsHolder.AddSpawnPointsIfNeed(linePosition);
            for (int i = 0; i < result; i++)
            {
                var spawnPoint = spawnPointPool.GetNewObject();
                var spawnPointIdOffset = result - i;
                spawnPoint.transform.position = spawnPointsHolder.SpawnPoints[spawnPointsHolder.SpawnPoints.Count - spawnPointIdOffset];
            }
            levelManager.UpdateMaxAvailableSoldiersCount(spawnPointsHolder.SpawnPoints.Count);
        }
    }
}