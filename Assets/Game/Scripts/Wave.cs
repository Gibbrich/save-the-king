using UnityEngine;

namespace Game.Scripts
{
    [CreateAssetMenu(menuName = "Game/Wave")]
    public class Wave : ScriptableObject
    {
        public int enemiesInRow;
        public int enemiesInColumn;
        public float spawnDelay;
    }
}