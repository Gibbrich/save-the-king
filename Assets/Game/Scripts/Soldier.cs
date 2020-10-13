using UnityEngine;

namespace Game.Scripts
{
    public class Soldier : MonoBehaviour
    {
        public Material normalMaterial;
        public Material spawnMaterial;
        public Renderer meshRenderer;

        public void ChangeMaterial(bool isSpawn)
        {
            var material = isSpawn ? spawnMaterial : normalMaterial;
            meshRenderer.material = material;
        }
    }
}