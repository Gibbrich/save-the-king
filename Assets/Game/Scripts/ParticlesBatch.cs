using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class ParticlesBatch : MonoBehaviour
    {
        private ParticleSystem[] systems;

        private void Start()
        {
            systems = GetComponentsInChildren<ParticleSystem>();
        }

        public void PlayAll()
        {
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].Play();
            }
        }
    }
}