using System;
using UnityEngine;

namespace Game.Scripts
{
    public class LevelManager : MonoBehaviour
    {
        public UIManager uiManager;
        public int maxAvailableSoldiers;

        public int SpawnedSoldiers { get; set; }

        private void Start()
        {
            UpdateMaxAvailableSoldiersCount(0);
        }

        public void UpdateMaxAvailableSoldiersCount(int upcomingSoldiers)
        {
            var availableSoldiersLeft = maxAvailableSoldiers - SpawnedSoldiers - upcomingSoldiers;
            uiManager.SetAvailableSoldiersToSpawnAmount(availableSoldiersLeft);
        }
    }
}