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

        private void Start()
        {
            spawners = GetComponentsInChildren<EnemySpawner>();
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
    }
}