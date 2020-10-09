using System;
using UnityEngine;

namespace Game.Scripts.ui
{
    [RequireComponent(typeof(Animator))]
    public class UiAnimatableElement : MonoBehaviour
    {
        private static readonly int State = Animator.StringToHash("State");

        public Animator animator;

        public event Action OnShow = () => { };
        public event Action OnHide = () => { };

        public void Show(bool withAnimation)
        {
            var state = withAnimation
                ? BaseUiElementState.SHOW
                : BaseUiElementState.APPEAR_WITHOUT_ANIMATION;
            
            animator.SetInteger(State, (int) state);
        }

        public void Hide(bool withAnimation)
        {
            var state = withAnimation
                ? BaseUiElementState.HIDE
                : BaseUiElementState.DISAPPEAR_WITHOUT_ANIMATION;
            
            animator.SetInteger(State, (int) state);
        }

        public void OnShowAnimationEnd() => OnShow.Invoke();

        public void OnHideAnimationEnd() => OnHide.Invoke();
    }
}