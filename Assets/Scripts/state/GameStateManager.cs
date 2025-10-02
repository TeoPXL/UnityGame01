using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSettings
{
    [SerializeField]
    private float _difficulty = 1f;

    public float difficulty
    {
        get => _difficulty;
        set => _difficulty = Mathf.Clamp(value, 0.5f, 1.5f); // min 0.5, max 1.5
    }

    public float volume = 1f;
    public bool enableSFX = true;
}

public class GameStateManager : MonoBehaviour
{
    [Header("UI Prefabs (assign concrete prefabs)")]
    public MenuUI menuUIPrefab;
    public PauseUI pauseUIPrefab;
    public OptionsUI optionsUIPrefab;
    public PlayingUI PlayingUIPrefab;

    [Header("Optional")]
    public Transform uiRoot; // parent instantiated UI under this (optional)

    public static GameStateManager Instance { get; private set; }

    private Stack<IGameState> stateStack = new Stack<IGameState>();

    public IGameState CurrentState => stateStack.Count > 0 ? stateStack.Peek() : null;

    public GameSettings settings = new GameSettings();

    // Event when the top-of-stack state changes (useful for systems that want to react)
    public event Action<IGameState> OnTopStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        CurrentState?.Tick(Time.deltaTime);
    }

    #region Stack API

    public void PushState(IGameState newState)
    {
        stateStack.Push(newState);
        newState.Enter();
        NotifyTopStateChanged();
    }

    public void PopState()
    {
        if (stateStack.Count == 0) return;

        var top = stateStack.Pop();
        try { top.Exit(); }
        catch (Exception e) { Debug.LogException(e); }

        NotifyTopStateChanged();
    }

    public void ChangeState(IGameState newState)
    {
        if (stateStack.Count > 0)
        {
            var top = stateStack.Pop();
            try { top.Exit(); } 
            catch (Exception e) { Debug.LogException(e); }
        }

        stateStack.Push(newState);
        newState.Enter();
        NotifyTopStateChanged();
    }

    /// <summary>
    /// Clears all states and replaces them with a new state.
    /// Used for Exit to Menu or similar full-reset transitions.
    /// </summary>
    public void ClearAndChangeState(IGameState newState)
    {
        while (stateStack.Count > 0)
        {
            var s = stateStack.Pop();
            try { s.Exit(); } 
            catch (Exception e) { Debug.LogException(e); }
        }

        PushState(newState);
    }

    private void NotifyTopStateChanged()
    {
        OnTopStateChanged?.Invoke(CurrentState);
    }

    #endregion

    #region UI Instantiation Helpers

    public T SpawnUI<T>(T prefab) where T : Component
    {
        if (prefab == null) throw new ArgumentNullException(nameof(prefab));
        var inst = Instantiate(prefab, uiRoot ? uiRoot : null);
        inst.gameObject.SetActive(true);
        return inst;
    }

    public void DestroyUI(Component ui)
    {
        if (ui == null) return;
        Destroy(ui.gameObject);
    }

    #endregion

    #region Coroutine Bridge

    public Coroutine StartManagedCoroutine(System.Collections.IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }

    public void StopManagedCoroutine(Coroutine c)
    {
        if (c != null) StopCoroutine(c);
    }

    #endregion

    #region Convenience / Query helpers

    // Check if the top-of-stack is of type T
    public bool IsTopState<T>() where T : IGameState
    {
        return CurrentState is T;
    }

    // Check if any state of type T exists anywhere on the stack
    public bool IsStateOnStack<T>() where T : IGameState
    {
        foreach (var s in stateStack) if (s is T) return true;
        return false;
    }

    // Category booleans (use marker interfaces or concrete types)
    public bool IsPaused => CurrentState is PauseState;
    public bool IsPlaying => CurrentState is IPlayingState;
    public bool IsMenu => CurrentState is IMenuState;
    public bool IsOptions => CurrentState is IOptionsState;

    #endregion

    #region Bootstrap

    [SerializeField] private bool startWithMenu = true;
    private void Start()
    {
        if (startWithMenu && menuUIPrefab != null)
        {
            ChangeState(new MenuState(menuUIPrefab));
        }
    }

    #endregion
}
