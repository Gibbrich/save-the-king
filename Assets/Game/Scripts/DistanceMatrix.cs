using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class DistanceMatrix
    {
        private readonly List<Vector3> from;
        private readonly List<Vector3> to;
        private float[,] matrix;
        private HashSet<int> occupiedToIndices;
        private HashSet<int> occupiedFromIndices;

        public DistanceMatrix(List<Vector3> from, List<Vector3> to)
        {
            this.from = from;
            this.to = to;
            occupiedToIndices = new HashSet<int>();
            occupiedFromIndices = new HashSet<int>();
            matrix = new float[from.Count, to.Count];
            for (int i = 0; i < from.Count; i++)
            {
                for (int j = 0; j < to.Count; j++)
                {
                    matrix[i, j] = (to[j] - from[i]).sqrMagnitude;
                }
            }
        }

        public SoldierDestination GetNearestDestination(List<int> soldiersIndices)
        {
            var smallestDistance = float.MaxValue;
            var toId = -1;
            var fromId = -1;
            for (int i = 0; i < soldiersIndices.Count; i++)
            {
                var soldierId = soldiersIndices[i];
                if (occupiedFromIndices.Contains(soldierId))
                {
                    continue;
                }
                
                for (int j = 0; j < to.Count; j++)
                {
                    if (occupiedToIndices.Contains(j))
                    {
                        continue;
                    }
                    
                    var distance = matrix[soldierId, j];
                    if (distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        toId = j;
                        fromId = soldierId;
                    }
                }
            }

            occupiedToIndices.Add(toId);
            occupiedFromIndices.Add(fromId);
            
            return new SoldierDestination(fromId, to[toId]);
        }
    }
}