using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class Level : MonoBehaviour
    {
        private static readonly Vector3 DEFAULT_KING_POSITION = new Vector3(-0.8f, 0f, -3.05f);
        
        public int maxAvailableSoldiers;
        public Vector3 startKingPosition = DEFAULT_KING_POSITION;
        public bool shouldShowTacticsTutorial;
        
        private EnemySpawner[] spawners;

        public int SpawnedSoldiers { get; private set; }

        public LevelPhase Phase { get; private set; } = LevelPhase.TACTIC;

        public int TotalEnemiesCount { get; private set; }

        public event Action OnLevelLoad = () => { };
        public event Action OnEnemyDeath = () => { };
        
        public event Action OnEnemyDeathTriggered = () => { };

        private void Start()
        {
            spawners = GetComponentsInChildren<EnemySpawner>();

            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].OnEnemyDeath += () => OnEnemyDeath.Invoke();
                spawners[i].OnEnemyDeathTriggered += () => OnEnemyDeathTriggered.Invoke();
            }

            TotalEnemiesCount = GetRemainedEnemies();
            OnLevelLoad.Invoke();
        }

        public void UpdateSpawnedSoldiers(int soldiersAmount)
        {
            SpawnedSoldiers += soldiersAmount;
        }

        public bool ShouldStartBattle() => SpawnedSoldiers == maxAvailableSoldiers;

        public void StartBattle()
        {
            Phase = LevelPhase.BATTLE;
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].OnBattleStart();
            }
        }

        public int GetAvailableSoldiersToSpawn() => maxAvailableSoldiers - SpawnedSoldiers;

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

        public void OnLevelReload()
        {
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].OnLevelReload();
            }
        }
    }
}