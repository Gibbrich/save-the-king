using System;
using Gamelogic.Extensions;
using UnityEngine;

namespace Game.Scripts
{
    public class EnemySpawner : MonoBehaviour
    {
        public LevelManager levelManager;
        public GameObject king;
        public OptimizedUnit enemyPrefab;
        public float distanceBetweenEnemies;
        public int enemiesInRow;
        public int enemiesInColumn;
        public float firstWaveSpawnDelay;
        public float nextWaveSpawnDelay;
        public float soldierRotationOffset = 90f;

        private Pool<OptimizedUnit> enemyPool;
        private float lastSpawnTime;
        private bool isFirstWave = true;

        private void Start()
        {
            enemyPool = new Pool<OptimizedUnit>(50, CreateEnemy, DestroyEnemy, WakeUpEnemy, SetToSleepEnemy);
        }

        private void Update()
        {
            if (levelManager.IsBattleStarted)
            {
                var delay = isFirstWave ? firstWaveSpawnDelay : nextWaveSpawnDelay;
                if (Time.timeSinceLevelLoad - lastSpawnTime >= delay)
                {
                    isFirstWave = false;
                    SpawnWave();
                }
            }
        }

        public void OnBattleStart()
        {
            lastSpawnTime = Time.timeSinceLevelLoad;
        }

        private void SpawnWave()
        {
            var position = new Vector2(transform.position.x, transform.position.z);

            var kingPosition = king.transform.position;
            var kingPosition2d = new Vector2(kingPosition.x, kingPosition.z);
            
            var direction = kingPosition2d - position;
            var angleDeg = soldierRotationOffset - Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

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
            unit.OnDeath = SetToSleepEnemy;
            return unit;
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