using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    // Assign these in the Inspector (buttons inside your Menu prefab)
    public Button startButton;
    public Button optionsButton;
    public Button quitButton;

    // Events the state code subscribes to
    public event Action onStartClicked;
    public event Action onOptionsClicked;
    public event Action onQuitClicked;

    private void Awake()
    {
        if (startButton != null) startButton.onClick.AddListener(() => onStartClicked?.Invoke());
        if (optionsButton != null) optionsButton.onClick.AddListener(() => onOptionsClicked?.Invoke());
        if (quitButton != null) quitButton.onClick.AddListener(() => onQuitClicked?.Invoke());
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
