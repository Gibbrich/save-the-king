using System.Collections;
using Game.Scripts.ui;
using UnityEngine;

namespace Game.Scripts.UI
{
    public class BattleStartController : MonoBehaviour
    {
        public float hideDelay;
        public UiAnimatableElement element;
        
        public void Show(bool withAnimation)
        {
            element.Show(withAnimation);
            StartCoroutine(HideWithDelay());
        }

        public void Hide(bool withAnimation)
        {
            element.Hide(withAnimation);
        }

        private IEnumerator HideWithDelay()
        {
            yield return new WaitForSecondsRealtime(hideDelay);
            Hide(true);
        }
    }
}