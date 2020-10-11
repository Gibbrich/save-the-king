using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class Level : MonoBehaviour
    {
        public int maxAvailableSoldiers;
        
        private EnemySpawner[] spawners;

        public int SpawnedSoldiers { get; private set; }

        public LevelPhase Phase { get; private set; } = LevelPhase.TACTIC;

        public int TotalEnemiesCount { get; private set; }

        public event Action OnLevelLoad = () => { };
        public event Action OnEnemyDeath = () => { };

        private void Start()
        {
            spawners = GetComponentsInChildren<EnemySpawner>();

            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].OnEnemyDeath += () => OnEnemyDeath.Invoke();
            }

            TotalEnemiesCount = GetRemainedEnemies();
            OnLevelLoad.Invoke();
        }

        public bool UpdateSpawnedSoldiers(int soldiersAmount)
        {
            SpawnedSoldiers += soldiersAmount;
            var shouldStartBattle = SpawnedSoldiers == maxAvailableSoldiers;
            if (shouldStartBattle)
            {
                Phase = LevelPhase.BATTLE;
                for (int i = 0; i < spawners.Length; i++)
                {
                    spawners[i].OnBattleStart();
                }
            }

            return shouldStartBattle;
        }

        public int GetSpawnPointsLimit() => maxAvailableSoldiers - SpawnedSoldiers;

        public int GetRemainedEnemies()
        {
            int remainedEnemies = 0;

            for (int i = 0; i < spawners.Length; i++)
            {
                var spawner = spawners[i];
                remainedEnemies += spawner.GetRemainedEnemiesCount();
            }

            return remainedEnemies;
        }

        public void OnKingDeath()
        {
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].OnKingDeath();
            }
        }
    }
}