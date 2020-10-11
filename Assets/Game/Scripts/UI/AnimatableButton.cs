using System;
using Game.Scripts.ui;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    [RequireComponent(typeof(UiAnimatableElement))]
    public class AnimatableButton : MonoBehaviour
    {
        public UiAnimatableElement element;
        public Button button;

        public event Action OnButtonClick = () => { };

        private void Start()
        {
            button.onClick.AddListener(() => OnButtonClick.Invoke());
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