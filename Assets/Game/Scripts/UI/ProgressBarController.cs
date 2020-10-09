using Game.Scripts.ui;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class ProgressBarController : MonoBehaviour
    {
        public Text currentLevelLabel;
        public Text nextLevelLabel;
        public UiAnimatableElement element;
        public Slider slider;
        
        public void UpdateLevelInfo(int currentLevel, int maxScore)
        {
            currentLevelLabel.text = currentLevel.ToString();
            nextLevelLabel.text = (currentLevel + 1).ToString();
            slider.maxValue = maxScore;
        }

        public void UpdateLevelProgress(int currentProgress)
        {
            slider.value = currentProgress;
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