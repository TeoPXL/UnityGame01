using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    // Assign these in the Inspector (buttons inside your Pause prefab)
    public Button resumeButton;
    public Button optionsButton;
    public Button exitToMenuButton;

    // Events the state code subscribes to
    public event Action onResumeClicked;
    public event Action onOptionsClicked;
    public event Action onExitToMenuClicked;

    private void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(() => onResumeClicked?.Invoke());
        if (optionsButton != null) optionsButton.onClick.AddListener(() => onOptionsClicked?.Invoke());
        if (exitToMenuButton != null) exitToMenuButton.onClick.AddListener(() => onExitToMenuClicked?.Invoke());
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
