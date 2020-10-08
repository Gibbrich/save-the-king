using JetBrains.Annotations;
using UnityEngine;

namespace Game.Scripts
{
    public interface TargetSeeker
    {
        [CanBeNull] Health GetTarget(GameObject[] potentialTargets);
    }
}