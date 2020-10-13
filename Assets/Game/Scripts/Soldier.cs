using UnityEngine;

namespace Game.Scripts
{
    public class Soldier : MonoBehaviour
    {
        public Material normalMaterial;
        public Material spawnMaterial;
        public Renderer meshRenderer;

        public bool isNormalMaterialActive = true;

        public bool ChangeMaterial(bool isSpawn)
        {
            var material = isSpawn ? spawnMaterial : normalMaterial;
            meshRenderer.material = material;

            var isChanged = isNormalMaterialActive == isSpawn;
            isNormalMaterialActive = !isSpawn;
            
            return isChanged;
        }
    }
}