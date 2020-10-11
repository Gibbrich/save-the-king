using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(Animator))]
    public class AnimatableCamera : MonoBehaviour
    {
        public Animator animator;
        
        private static readonly int State = Animator.StringToHash("State");
        private const int LEVEL_LOAD_STATE = 0;
        private const int BATTLE_START_STATE = 1;
        private const int LEVEL_END_STATE = 2;

        public void OnLevelLoad()
        {
            animator.SetInteger(State, LEVEL_LOAD_STATE);
        }

        public void OnBattleStart()
        {
            animator.SetInteger(State, BATTLE_START_STATE);
        }

        public void OnLevelEnd()
        {
            animator.SetInteger(State, LEVEL_END_STATE);
        }
    }
}