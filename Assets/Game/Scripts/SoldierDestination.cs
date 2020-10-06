using UnityEngine;

namespace Game.Scripts
{
    public readonly struct SoldierDestination
    {
        public readonly int soldierId;
        public readonly Vector3 destination;

        public SoldierDestination(int soldierId, Vector3 destination)
        {
            this.soldierId = soldierId;
            this.destination = destination;
        }
    }
}