using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class SpawnPointsHolder
    {
        private readonly float spawnPointDistance;
        public List<Vector3> SpawnPoints;
        private float spawnPointDistanceSquare;

        public SpawnPointsHolder(float spawnPointDistance)
        {
            this.spawnPointDistance = spawnPointDistance;
            spawnPointDistanceSquare = Mathf.Pow(spawnPointDistance, 2);
            SpawnPoints = new List<Vector3>();
        }

        public int AddSpawnPointsIfNeed(Vector3 linePosition)
        {
            if (SpawnPoints.Count == 0)
            {
                SpawnPoints.Add(linePosition);
                return 1;
            }
            else
            {
                var result = 0;
                while (ShouldAddSpawnPoint(linePosition))
                {
                    var previousSpawnPoint = SpawnPoints[SpawnPoints.Count - 1];
                    // can't use squared vector length here as calculations incorrect :(
                    var distanceToPreviousSpawnPoint = (linePosition - previousSpawnPoint).magnitude;
                    var distanceToNewSpawnPoint = distanceToPreviousSpawnPoint - spawnPointDistance;
                    var lerpCoefficient = distanceToNewSpawnPoint / distanceToPreviousSpawnPoint;
                    var newSpawnPoint = Vector3.Lerp(linePosition, previousSpawnPoint, lerpCoefficient);

                    SpawnPoints.Add(newSpawnPoint);
                    result++;
                }

                return result;
            }
        }

        private bool ShouldAddSpawnPoint(Vector3 linePosition)
        {
            var previousSpawnPoint = SpawnPoints[SpawnPoints.Count - 1];
            var distanceToPreviousSpawnPointSquare = (linePosition - previousSpawnPoint).sqrMagnitude;
            return distanceToPreviousSpawnPointSquare >= spawnPointDistanceSquare;
        }
    }
}