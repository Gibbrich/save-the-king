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
        public OptimizedUnit soldierPrefab;
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
        private Pool<OptimizedUnit> soldiersPool;
        private GameObject spawnPointsParent;
        private GameObject soldiersParent;
        private SpawnPointsHolder spawnPointsHolder;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            points = new List<Vector3>();
            spawnPointsHolder = new SpawnPointsHolder(spawnPointDistance);
            minDistanceSquare = Mathf.Pow(minPointDistance, 2);
            camera = Camera.main;
            
            spawnPointsParent = new GameObject("spawnPointsParent");
            spawnPointsParent.transform.parent = transform;
            
            soldiersParent = new GameObject("soldiersParent");
            soldiersParent.transform.parent = transform;
            
            spawnPointPool = new Pool<GameObject>(50, CreateSpawnPoint, Destroy, WakeUpSpawnPoint, SetToSleepSpawnPoint);
            soldiersPool = new Pool<OptimizedUnit>(50, CreateSoldier, DestroySoldier, WakeUpSoldier, SetAsleepSoldier);
        }

        private void Update()
        {
            switch (levelManager.Phase)
            {
                case LevelPhase.TACTIC:
                    TacticPhaseUpdate();
                    break;
                case LevelPhase.BATTLE:
                    BattlePhaseUpdate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void TacticPhaseUpdate()
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
                        ResetSpawnLineState();
                    }
                }
                else
                {
                    if (spawnPointsHolder.SpawnPoints.Count < levelManager.GetSpawnPointsLimit())
                    {
                        AddPointIfNeed(touch.position);
                    }
                }
            }
        }

        private void ResetSpawnLineState()
        {
            spawnPointPool.ReleaseAll();
            points.Clear();
            spawnPointsHolder.SpawnPoints.Clear();
            lineRenderer.positionCount = 0;
        }

        private void BattlePhaseUpdate()
        {
            var touchCount = TouchManager.TouchCount;
            if (touchCount > 0)
            {
                var touch = TouchManager.GetTouch(0);
                if (touch.phase == TouchPhase.Ended)
                {
                    if (spawnPointsHolder.SpawnPoints.Count > 0)
                    {
                        MoveSoldiers();
                        ResetSpawnLineState();
                    }
                }
                else
                {
                    if (spawnPointsHolder.SpawnPoints.Count < soldiersPool.GetActiveObjectsCount())
                    {
                        AddPointIfNeed(touch.position);
                    }
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

        private void DestroySoldier(OptimizedUnit soldier)
        {
            soldier.OnDeath = null;
            Destroy(soldier.gameObject);
        }

        private OptimizedUnit CreateSoldier()
        {
            var unit = Instantiate(soldierPrefab, soldiersParent.transform);
            unit.Disable();
            unit.OnDeath = ReleaseToPool;
            return unit;
        }

        private void ReleaseToPool(OptimizedUnit unit) => soldiersPool.Release(unit);

        private void WakeUpSoldier(OptimizedUnit soldier)
        {
            soldier.gameObject.SetActive(true);
        }

        private void SetAsleepSoldier(OptimizedUnit soldier)
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

            if (levelManager.Phase == LevelPhase.TACTIC)
            {
                levelManager.UpdateMaxAvailableSoldiersCount(spawnPointsHolder.SpawnPoints.Count);
            }
        }

        private int GetSpawnPointsLimit()
        {
            switch (levelManager.Phase)
            {
                case LevelPhase.TACTIC:
                    return levelManager.GetSpawnPointsLimit();
                case LevelPhase.BATTLE:
                    return soldiersPool.GetActiveObjectsCount();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MoveSoldiers()
        {
            var aliveSoldiers = soldiersPool.GetActiveObjects();
            var aliveSoldiersPositions = aliveSoldiers.Select(unit => unit.transform.position).ToList();
            var matrix = new DistanceMatrix(aliveSoldiersPositions, spawnPointsHolder.SpawnPoints);
            var idleSoldiersIds = new List<int>();
            var commonSoldierIds = new List<int>();
            for (int i = 0; i < aliveSoldiers.Count; i++)
            {
                if (aliveSoldiers[i].GetState() == SoldierState.IDLE)
                {
                    idleSoldiersIds.Add(i);
                }
                else
                {
                    commonSoldierIds.Add(i);
                }
            }

            for (int i = 0; i < spawnPointsHolder.SpawnPoints.Count; i++)
            {
                // first move idle soldiers then use other soldier ids
                var soldiersIds = idleSoldiersIds.Count > 0 ? idleSoldiersIds : commonSoldierIds;
                SetNavigationTarget(soldiersIds, matrix, aliveSoldiers);
            }
        }

        private void SetNavigationTarget(
            List<int> soldierIds, 
            DistanceMatrix matrix, 
            List<OptimizedUnit> aliveSoldiers)
        {
            var destination = matrix.GetNearestDestination(soldierIds);
            var soldier = aliveSoldiers[destination.soldierId];
            soldier.MoveToTarget(destination.destination, true);
            soldierIds.Remove(destination.soldierId);
        }
    }
}