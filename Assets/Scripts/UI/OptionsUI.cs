using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public Button closeButton;

    public Slider difficultySlider;

    public event Action onCloseClicked;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => onCloseClicked?.Invoke());
    }

    private void Start()
    {
        if (difficultySlider != null)
        {
            // Initialize slider from current setting
            difficultySlider.value = GameStateManager.Instance.settings.difficulty;

            // Update settings when slider changes
            difficultySlider.onValueChanged.AddListener(val =>
            {
                GameStateManager.Instance.settings.difficulty = val;
            });
        }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
