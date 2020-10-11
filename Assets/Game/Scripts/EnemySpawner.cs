using System;
using System.Collections.Generic;
using Gamelogic.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts
{
    public class EnemySpawner : MonoBehaviour
    {
        private LevelManager levelManager;
        public OptimizedUnit enemyPrefab;
        public float distanceBetweenEnemies;
        public List<Wave> waves;
        public float soldierRotationOffset = 90f;

        private Pool<OptimizedUnit> enemyPool;
        private float lastSpawnTime;
        private int nextWaveId = 0;
        private King king;

        public event Action OnEnemyDeath = () => { };

        private void Start()
        {
            enemyPool = new Pool<OptimizedUnit>(50, CreateEnemy, DestroyEnemy, WakeUpEnemy, SetToSleepEnemy);
            king = FindObjectOfType<King>();
            levelManager = FindObjectOfType<LevelManager>();
        }

        private void Update()
        {
            if (levelManager.CurrentLevel.Phase == LevelPhase.BATTLE && 
                waves.Count > 0 && 
                nextWaveId < waves.Count && 
                Time.timeSinceLevelLoad - lastSpawnTime >= waves[nextWaveId].spawnDelay &&
                king && !king.unit.health.IsDead())
            {
                SpawnWave();
                nextWaveId++;
                lastSpawnTime = Time.timeSinceLevelLoad;
            }
        }

        public int GetRemainedEnemiesCount()
        {
            var currentlyActive = enemyPool.GetActiveObjectsCount();
            for (int i = nextWaveId; i < waves.Count; i++)
            {
                var wave = waves[i];
                currentlyActive += wave.enemiesInColumn * wave.enemiesInRow;
            }

            return currentlyActive;
        }

        public void OnBattleStart()
        {
            lastSpawnTime = Time.timeSinceLevelLoad;
        }

        public void OnKingDeath()
        {
            var optimizedUnits = enemyPool.GetActiveObjects();
            for (int i = 0; i < optimizedUnits.Count; i++)
            {
                optimizedUnits[i].OnVictory();
            }
        }

        private void SpawnWave()
        {
            var position = new Vector2(transform.position.x, transform.position.z);

            var kingPosition = king.transform.position;
            var kingPosition2d = new Vector2(kingPosition.x, kingPosition.z);
            
            var direction = kingPosition2d - position;
            var angleDeg = soldierRotationOffset - Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            var enemiesInRow = waves[nextWaveId].enemiesInRow;
            var enemiesInColumn = waves[nextWaveId].enemiesInColumn;
            
            var groupOffsetX = (enemiesInRow - 1) * distanceBetweenEnemies / 2;
            var groupOffsetZ = (enemiesInColumn - 1) * distanceBetweenEnemies / 2;
            var groupOffset = new Vector3(groupOffsetX, 0, groupOffsetZ);
            
            for (int i = 0; i < enemiesInRow; i++)
            {
                for (int j = 0; j < enemiesInColumn; j++)
                {
                    var offset = new Vector3(i * distanceBetweenEnemies, 0, j * distanceBetweenEnemies);
                    var enemyPosition = transform.position + offset - groupOffset;
                    var enemy = enemyPool.GetNewObject();
                    
                    enemy.transform.position = enemyPosition;
                    enemy.transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.up);
                }
            }
        }

        private OptimizedUnit CreateEnemy()
        {
            var unit = Instantiate(enemyPrefab, transform);
            unit.Disable();
            unit.OnDeath = ReleaseToPool;
            return unit;
        }
        
        private void ReleaseToPool(OptimizedUnit unit)
        {
            enemyPool.Release(unit);
            OnEnemyDeath.Invoke();
        }

        private void DestroyEnemy(OptimizedUnit enemy)
        {
            enemy.OnDeath = null;
            Destroy(enemy.gameObject);
        }

        private void WakeUpEnemy(OptimizedUnit enemy)
        {
            enemy.gameObject.SetActive(true);
            enemy.Enable();
        }
        
        private void SetToSleepEnemy(OptimizedUnit enemy)
        {
            enemy.Disable();
            enemy.gameObject.SetActive(false);
        }
    }
}