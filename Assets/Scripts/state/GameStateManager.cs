using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace state
{
    [Serializable]
    public class GameSettings
    {
        [SerializeField] private float difficulty = 1f;

        public float Difficulty
        {
            get => difficulty;
            set => difficulty = Mathf.Clamp(value, 0.5f, 1.5f); // min 0.5, max 1.5
        }
    }

    public class GameStateManager : MonoBehaviour
    {
        [Header("UI Prefabs (assign concrete prefabs)")]
        public MenuUI menuUIPrefab;

        public PauseUI pauseUIPrefab;
        public OptionsUI optionsUIPrefab;
        public PlayingUI playingUIPrefab;
        public ScoreUI scoreUIPrefab;
        private int score;
        public int Score
        {
            get => score;
            set
            {
                score = value; 
                OnScoreChanged?.Invoke(score);
            }
            
        }
        public event UnityAction<int> OnScoreChanged;

        [Header("Optional")] public Transform uiRoot; // parent instantiated UI under this (optional)

        public static GameStateManager Instance { get; private set; }

        private Stack<State> stateStack = new();

        public State CurrentState => stateStack.Count > 0 ? stateStack.Peek() : null;

        public GameSettings settings = new();

        // Event when the top-of-stack state changes (useful for systems that want to react)
        public event Action<State> OnTopStateChanged;

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
            CurrentState?.Tick();
        }

        #region Stack API

        public void PushState(State newState)
        {
            stateStack.Push(newState);
            newState.Enter();
            NotifyTopStateChanged();
        }

        public void PopState()
        {
            if (stateStack.Count == 0) return;

            var top = stateStack.Pop();
            try
            {
                top.Exit();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            NotifyTopStateChanged();
        }

        public void ChangeState(State newState)
        {
            if (stateStack.Count > 0)
            {
                var top = stateStack.Pop();
                try
                {
                    top.Exit();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            stateStack.Push(newState);
            newState.Enter();
            NotifyTopStateChanged();
        }

        /// <summary>
        /// Clears all states and replaces them with a new state.
        /// Used for Exit to Menu or similar full-reset transitions.
        /// </summary>
        public void ClearAndChangeState(State newState)
        {
            while (stateStack.Count > 0)
            {
                var s = stateStack.Pop();
                try
                {
                    s.Exit();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
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

        // Category booleans (use marker interfaces or concrete types)
        public bool IsPaused => (CurrentState.GameState == GameState.Paused) ||  (CurrentState.GameState == GameState.Options);
        public bool IsPlaying => CurrentState.GameState == GameState.Playing;
        public bool IsMenu => CurrentState.GameState == GameState.Menu;
        public bool IsOptions => CurrentState.GameState == GameState.Options;

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
}