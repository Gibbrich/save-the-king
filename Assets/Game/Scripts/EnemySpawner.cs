using System;
using Gamelogic.Extensions;
using UnityEngine;

namespace Game.Scripts
{
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject king;
        public GameObject enemyPrefab;
        public float distanceBetweenEnemies;
        public int enemiesInRow;
        public int enemiesInColumn;
        public float firstWaveSpawnDelay;
        public float nextWaveSpawnDelay;
        public float soldierRotationOffset = 90f;

        private Pool<GameObject> enemyPool;
        private float lastSpawnTime;
        private bool isFirstWave = true;

        private void Start()
        {
            enemyPool = new Pool<GameObject>(50, CreateEnemy, Destroy, WakeUpEnemy, SetToSleepEnemy);
            lastSpawnTime = Time.timeSinceLevelLoad;
        }

        private void Update()
        {
            var delay = isFirstWave ? firstWaveSpawnDelay : nextWaveSpawnDelay;
            if (Time.timeSinceLevelLoad - lastSpawnTime >= delay)
            {
                isFirstWave = false;
                SpawnWave();
            }
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

        private GameObject CreateEnemy() => Instantiate(enemyPrefab, transform);

        private void WakeUpEnemy(GameObject enemy)
        {
            enemy.SetActive(true);
        }
        
        private void SetToSleepEnemy(GameObject enemy)
        {
            enemy.SetActive(false);
        }
    }
}