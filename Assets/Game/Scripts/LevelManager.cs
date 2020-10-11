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
        private int visibleLevel;
        private UIManager uiManager;
        private PlayerSoldierSpawnManager playerSoldierSpawnManager;
        private King king;

        public Level CurrentLevel { get; private set; }

        private void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            playerSoldierSpawnManager = FindObjectOfType<PlayerSoldierSpawnManager>();
            king = FindObjectOfType<King>();
            king.OnKingDeath += OnKingDeath;
            LoadNextLevel();
        }
        
        public void UpdateSpawnedSoldiers(int soldiersAmount)
        {
            var result = CurrentLevel.UpdateSpawnedSoldiers(soldiersAmount);
            if (result)
            {
                playerSoldierSpawnManager.OnBattleStart();
                uiManager.SetState(new UIManager.UIManagerState.StartBattle());
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
            visibleLevel++;
            if (nextLevelId >= levels.Count)
            {
                nextLevelId = 0;
            }
            
            uiManager.SetState(new UIManager.UIManagerState.PlaceHumans());
            king.Refresh();
        }

        private void OnLevelLoad()
        {
            uiManager.UpdateLevelInfo(visibleLevel, CurrentLevel.TotalEnemiesCount);
            uiManager.UpdateLevelProgress(0);
            CurrentLevel.OnLevelLoad -= OnLevelLoad;
        }

        private void OnEnemyDeath()
        {
            var remainedEnemies = CurrentLevel.GetRemainedEnemies();
            uiManager.UpdateLevelProgress(CurrentLevel.TotalEnemiesCount - remainedEnemies);

            if (remainedEnemies == 0)
            {
                uiManager.SetState(new UIManager.UIManagerState.Victory());
                playerSoldierSpawnManager.OnLevelComplete();
                king.OnVictory();
                // StartCoroutine(ScheduleNextLevelLoad());
            }
        }

        private IEnumerator ScheduleNextLevelLoad()
        {
            yield return new WaitForSecondsRealtime(nextLevelLoadDelay);
            LoadNextLevel();
            playerSoldierSpawnManager.OnLevelStart();
        }

        private void OnKingDeath()
        {
            uiManager.SetState(new UIManager.UIManagerState.Loose());
            CurrentLevel.OnKingDeath();
        }
    }
}