using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuUI : MonoBehaviour
    {
        // Assign these in the Inspector (buttons inside your Menu prefab)
        public Button startButton;
        public Button optionsButton;
        public Button quitButton;

        // Events the state code subscribes to
        public event Action OnStartClicked;
        public event Action OnOptionsClicked;
        public event Action OnQuitClicked;

        private void Awake()
        {
            if (startButton != null) startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
            if (optionsButton != null) optionsButton.onClick.AddListener(() => OnOptionsClicked?.Invoke());
            if (quitButton != null) quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        }
    }
}