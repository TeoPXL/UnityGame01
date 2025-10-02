using System;
using state;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OptionsUI : MonoBehaviour
    {
        public Button closeButton;

        public Slider difficultySlider;

        public event Action OnCloseClicked;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        private void Start()
        {
            if (difficultySlider != null)
            {
                // Initialize slider from current setting
                difficultySlider.value = GameStateManager.Instance.settings.Difficulty;

                // Update settings when slider changes
                difficultySlider.onValueChanged.AddListener(val =>
                {
                    GameStateManager.Instance.settings.Difficulty = val;
                });
            }
        }
    }
}