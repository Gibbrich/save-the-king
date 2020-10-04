using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class LevelManager : MonoBehaviour
    {
        public UIManager uiManager;
        public int maxAvailableSoldiers;
        public List<EnemySpawner> spawners;
        public PlayerSoldierSpawnManager playerSoldierSpawnManager;

        public int SpawnedSoldiers { get; private set; }

        public bool IsBattleStarted { get; private set; }

        private void Start()
        {
            UpdateMaxAvailableSoldiersCount(0);
        }

        public void UpdateMaxAvailableSoldiersCount(int upcomingSoldiers)
        {
            var availableSoldiersLeft = maxAvailableSoldiers - SpawnedSoldiers - upcomingSoldiers;
            uiManager.SetAvailableSoldiersToSpawnAmount(availableSoldiersLeft);
        }

        public void UpdateSpawnedSoldiers(int soldiersAmount)
        {
            SpawnedSoldiers += soldiersAmount;
            if (SpawnedSoldiers == maxAvailableSoldiers)
            {
                IsBattleStarted = true;
                for (int i = 0; i < spawners.Count; i++)
                {
                    spawners[i].OnBattleStart();
                }
                playerSoldierSpawnManager.OnBattleStart();
            }
        }
    }
}