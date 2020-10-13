using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class PlayerSoldierSpawnManager : MonoBehaviour
    {
        public GameObject king;
        public GameObject spawnPointPrefab;
        public OptimizedUnit soldierPrefab;
        public float minPointDistance;
        public float lineRenderHeight = 0.5f;
        public float spawnPointDistance = 0.5f;
        public float soldierRotationOffset = 90f;

        private LevelManager levelManager;
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
            levelManager = FindObjectOfType<LevelManager>();
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
            switch (levelManager.CurrentLevel.Phase)
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

        public void OnLevelComplete()
        {
            var soldiers = soldiersPool.GetActiveObjects();
            for (int i = 0; i < soldiers.Count; i++)
            {
                soldiers[i].OnVictory();
            }
        }

        public void OnLevelStart()
        {
            var soldiers = soldiersPool.GetActiveObjects();
            for (int i = 0; i < soldiers.Count; i++)
            {
                StartCoroutine(soldiers[i].die());
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
                        ResetSpawnLineState();
                        levelManager.StartBattleIfNeed();
                    }
                }
                else
                {
                    var b = levelManager.CurrentLevel.GetAvailableSoldiersToSpawn() > 0;
                    if (!EventSystem.current.IsPointerOverGameObject() && b)
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
                    if (!EventSystem.current.IsPointerOverGameObject() && spawnPointsHolder.SpawnPoints.Count < soldiersPool.GetActiveObjectsCount())
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

            // set last line renderer point position equal spawn point position
            // to avoid tail without spawn point
            var spawnPointsAdded = AddSpawnPointsIfNeed(worldPoint);
            if (spawnPointsAdded > 0)
            {
                var lastSpawnPoint = spawnPointsHolder.SpawnPoints[spawnPointsHolder.SpawnPoints.Count - 1];
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, lastSpawnPoint);
            }
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

        private void ReleaseToPool(OptimizedUnit unit, bool shouldNotifyDeath) => soldiersPool.Release(unit);

        private void WakeUpSoldier(OptimizedUnit soldier)
        {
            soldier.gameObject.SetActive(true);
        }

        private void SetAsleepSoldier(OptimizedUnit soldier)
        {
            soldier.Disable();
            soldier.gameObject.SetActive(false);
        }

        private int AddSpawnPointsIfNeed(Vector3 linePosition)
        {
            var result = spawnPointsHolder.AddSpawnPointsIfNeed(linePosition, GetSpawnPointsLimit());
            for (int i = 0; i < result; i++)
            {
                var spawnPoint = spawnPointPool.GetNewObject();
                var spawnPointIdOffset = result - i;
                var id = spawnPointsHolder.SpawnPoints.Count - spawnPointIdOffset;
                spawnPoint.transform.position = spawnPointsHolder.SpawnPoints[id];
                
                if (levelManager.CurrentLevel.Phase == LevelPhase.TACTIC)
                {
                    var point = spawnPointsHolder.SpawnPoints[id];
                    var soldier = soldiersPool.GetNewObject();
                    soldier.transform.position = point;
                    soldier.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
                    soldier.SetVisibility(true);
                    
                    levelManager.UpdateSpawnedSoldiers(1);
                    levelManager.UpdateMaxAvailableSoldiersCount();
                }
            }

            return result;
        }

        private int GetSpawnPointsLimit()
        {
            switch (levelManager.CurrentLevel.Phase)
            {
                case LevelPhase.TACTIC:
                    return levelManager.CurrentLevel.GetAvailableSoldiersToSpawn();
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
            soldier.MoveToNavigationTarget(destination.destination);
            soldierIds.Remove(destination.soldierId);
        }
    }
}