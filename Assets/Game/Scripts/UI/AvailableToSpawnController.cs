using Game.Scripts.ui;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class AvailableToSpawnController : MonoBehaviour
    {
        public Text value;
        public UiAnimatableElement element;
        
        public void SetAvailableSoldiersToSpawnAmount(int amount)
        {
            value.text = amount.ToString();
        }

        public void Show(bool withAnimation)
        {
            element.Show(withAnimation);
        }
        
        public void Hide(bool withAnimation)
        {
            element.Hide(withAnimation);
        }
    }
}