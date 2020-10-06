using System.Collections;
using System.Collections.Generic;
using Game.Scripts;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SpawnPointTest
    {
        private SpawnPointsHolder holder;

        // A Test behaves as an ordinary method
        [Test]
        public void SpawnPointTestSimplePasses()
        {
            holder = new SpawnPointsHolder(0.35f);

            holder.AddSpawnPointsIfNeed(new Vector3(-1.7f, 0.2f, 6.0f), 5);
            holder.AddSpawnPointsIfNeed(new Vector3(-0.2f, 0.2f, 7.7f), 5);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator SpawnPointTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
