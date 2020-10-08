using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts
{
    public class AngerEmojiController : MonoBehaviour
    {
        public ParticleSystem angryEmojiEffect;
        [Range(0, 1)]
        public float angerEmojiSpawnProbability = 0.3f;
        public float spawnInterval = 1f;

        public bool CanSpawnEmoji { get; set; }

        private float lastSpawnTime;

        private void Update()
        {
            if (CanSpawnEmoji && Time.timeSinceLevelLoad - lastSpawnTime >= spawnInterval)
            {
                var spawnTry = Random.Range(0f, 1f);
                if (spawnTry <= angerEmojiSpawnProbability)
                {
                    angryEmojiEffect.Play();
                    lastSpawnTime = Time.timeSinceLevelLoad;
                }
            }
        }
    }
}