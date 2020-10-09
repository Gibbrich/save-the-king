using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class LevelManager : MonoBehaviour
    {
        public List<Level> levels;
        private int currentLevelId;
        private UIManager uiManager;
        private PlayerSoldierSpawnManager playerSoldierSpawnManager;

        public Level CurrentLevel { get; private set; }

        private void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            playerSoldierSpawnManager = FindObjectOfType<PlayerSoldierSpawnManager>();
            LoadNextLevel();
            UpdateMaxAvailableSoldiersCount(0);
        }
        
        public void UpdateSpawnedSoldiers(int soldiersAmount)
        {
            var result = CurrentLevel.UpdateSpawnedSoldiers(soldiersAmount);
            if (result)
            {
                playerSoldierSpawnManager.OnBattleStart();
            }
        }
        
        public void UpdateMaxAvailableSoldiersCount(int upcomingSoldiers)
        {
            var availableSoldiersLeft = CurrentLevel.GetSpawnPointsLimit() - upcomingSoldiers;
            uiManager.SetAvailableSoldiersToSpawnAmount(availableSoldiersLeft);
        }

        private void LoadNextLevel()
        {
            CurrentLevel = Instantiate(levels[currentLevelId]);
            CurrentLevel.OnLevelLoad += OnLevelLoad;
            CurrentLevel.OnEnemyDeath += OnEnemyDeath;
        }

        private void OnLevelLoad()
        {
            // todo
            CurrentLevel.OnLevelLoad -= OnLevelLoad;
        }

        private void OnEnemyDeath()
        {
            // todo
        }
    }
}