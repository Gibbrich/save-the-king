using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class LevelManager : MonoBehaviour
    {
        public List<Level> levels;
        public float nextLevelLoadDelay = 2f;
        private int nextLevelId = 0;
        private UIManager uiManager;
        private PlayerSoldierSpawnManager playerSoldierSpawnManager;
        private King king;

        public Level CurrentLevel { get; private set; }

        private void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            playerSoldierSpawnManager = FindObjectOfType<PlayerSoldierSpawnManager>();
            king = FindObjectOfType<King>();
            LoadNextLevel();
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
            if (CurrentLevel)
            {
                Destroy(CurrentLevel.gameObject);
            }
            CurrentLevel = Instantiate(levels[nextLevelId]);
            UpdateMaxAvailableSoldiersCount(0);
            CurrentLevel.OnLevelLoad += OnLevelLoad;
            CurrentLevel.OnEnemyDeath += OnEnemyDeath;
            nextLevelId++;
            if (nextLevelId >= levels.Count)
            {
                nextLevelId = 0;
            }
        }

        private void OnLevelLoad()
        {
            // todo - update UI - level progress
            CurrentLevel.OnLevelLoad -= OnLevelLoad;
        }

        private void OnEnemyDeath()
        {
            // todo - update level progress
            if (CurrentLevel.GetRemainedEnemies() == 0)
            {
                playerSoldierSpawnManager.OnLevelComplete();
                // todo - launch level complete king animation
                // refresh King hp
                // todo - set king position to default one
                king.unit.Enable();

                StartCoroutine(ScheduleNextLevelLoad());
            }
        }

        private IEnumerator ScheduleNextLevelLoad()
        {
            yield return new WaitForSecondsRealtime(nextLevelLoadDelay);
            LoadNextLevel();
        }
    }
}