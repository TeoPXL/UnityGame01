using UI;
using UnityEngine;

namespace state
{
    public abstract class State
    {
        public abstract GameState GameState { get; }
        public abstract void Enter();
        public abstract void Tick();
        public abstract void Exit();
    }

    #region MenuState

    public class MenuState : State
    {
        public override GameState GameState => GameState.Menu;

        private MenuUI prefab;
        private MenuUI instance;

        public MenuState(MenuUI prefab)
        {
            this.prefab = prefab;
        }

        public override void Enter()
        {
            instance = GameStateManager.Instance.SpawnUI(prefab);
            instance.OnStartClicked += HandleStart;
            instance.OnOptionsClicked += HandleOptions;
            instance.OnQuitClicked += HandleQuit;
        }

        public override void Tick()
        {
        }

        public override void Exit()
        {
            if (instance != null)
            {
                instance.OnStartClicked -= HandleStart;
                instance.OnOptionsClicked -= HandleOptions;
                instance.OnQuitClicked -= HandleQuit;

                GameStateManager.Instance.DestroyUI(instance);
                instance = null;
            }
        }

        private void HandleStart()
        {
            GameStateManager.Instance.ChangeState(
                new PlayingState(
                    GameStateManager.Instance.playingUIPrefab,
                    GameStateManager.Instance.scoreUIPrefab
                )
            );
        }

        private void HandleOptions()
        {
            GameStateManager.Instance.PushState(new OptionsState(GameStateManager.Instance.optionsUIPrefab));
        }

        private void HandleQuit()
        {
            Application.Quit();
        }
    }

    #endregion

    #region PlayingState

    public class PlayingState : State
    {
        private PlayingUI playingUIPrefab;
        private ScoreUI scoreUIPrefab;
        private PlayingUI playingUIInstance;
        private ScoreUI scoreUIInstance;
        public override GameState GameState => GameState.Playing;

        private int round = 1;
        private int maxAmmo;
        private int currentAmmo;
        private AmmoCounterUI ammoUI;

        public PlayingState(PlayingUI playingUIPrefab, ScoreUI scoreUIPrefab)
        {
            this.playingUIPrefab = playingUIPrefab;
            this.scoreUIPrefab = scoreUIPrefab;
        }

        public override void Enter()
        {
            playingUIInstance = GameStateManager.Instance.SpawnUI(playingUIPrefab);
            scoreUIInstance = GameStateManager.Instance.SpawnUI(scoreUIPrefab);
            // We should then use scoreUIInstance to modify actual score values

            ammoUI = playingUIInstance.GetComponentInChildren<AmmoCounterUI>();
            NozzleController nozzle = Object.FindFirstObjectByType<NozzleController>();
            maxAmmo = nozzle != null ? nozzle.maxAmmo : 6;
            currentAmmo = maxAmmo;
            UpdateAmmoUI();

            Debug.Log($"Playing - Round {round}");
        }

        public override void Tick()
        {
        }

        public override void Exit()
        {
            GameStateManager.Instance.DestroyUI(playingUIInstance);
            GameStateManager.Instance.DestroyUI(scoreUIInstance);
            playingUIInstance = null;
            scoreUIInstance = null;
        }

        public void Tick(float deltaTime)
        {
            if (InputUtils.WasPausePressedThisFrame())
            {
                GameStateManager.Instance.PushState(new PauseState(GameStateManager.Instance.pauseUIPrefab));
            }
        }

        public bool TryShoot(bool infinite = false)
        {
            if (currentAmmo <= 0 && !infinite) return false;

            if (!infinite) currentAmmo--;
            UpdateAmmoUI();
            return true;
        }

        public void Reload()
        {
            currentAmmo = maxAmmo;
            UpdateAmmoUI();
        }

        private void UpdateAmmoUI()
        {
            ammoUI?.UpdateAmmoDisplay(currentAmmo);
        }

        public void StartNextRound()
        {
            round++;
            if (round % 5 == 0)
            {
                GameStateManager.Instance.PushState(new BossState(round));
            }
            else
            {
                currentAmmo = maxAmmo;
                UpdateAmmoUI();
                Debug.Log("Starting round " + round);
            }
        }
    }

    #endregion

    #region PauseState

    public class PauseState : State
    {
        private PauseUI prefab;
        private PauseUI instance;
        public override GameState GameState => GameState.Paused;

        public PauseState(PauseUI prefab)
        {
            this.prefab = prefab;
        }

        public override void Enter()
        {
            instance = GameStateManager.Instance.SpawnUI(prefab);
            instance.OnResumeClicked += Resume;
            instance.OnOptionsClicked += OpenOptions;
            instance.OnExitToMenuClicked += ExitToMenu;
        }

        public override void Tick()
        {
        }

        public override void Exit()
        {
            if (instance != null)
            {
                instance.OnResumeClicked -= Resume;
                instance.OnOptionsClicked -= OpenOptions;
                instance.OnExitToMenuClicked -= ExitToMenu;

                GameStateManager.Instance.DestroyUI(instance);
                instance = null;
            }
        }

        public void ShowUI()
        {
            instance?.gameObject.SetActive(true);
        }

        public void Tick(float deltaTime)
        {
            if (InputUtils.WasPausePressedThisFrame())
            {
                Resume();
            }
        }

        private void Resume()
        {
            GameStateManager.Instance.PopState();
        }

        private void OpenOptions()
        {
            instance?.gameObject.SetActive(false);
            GameStateManager.Instance.PushState(new OptionsState(GameStateManager.Instance.optionsUIPrefab));
        }

        private void ExitToMenu()
        {
            Debug.Log("Exiting to menu");
            GameStateManager.Instance.ClearAndChangeState(new MenuState(GameStateManager.Instance.menuUIPrefab));
        }
    }

    #endregion

    #region OptionsState

    public class OptionsState : State
    {
        private OptionsUI prefab;
        private OptionsUI instance;
        public override GameState GameState => GameState.Options;

        public OptionsState(OptionsUI prefab)
        {
            this.prefab = prefab;
        }

        public override void Enter()
        {
            instance = GameStateManager.Instance.SpawnUI(prefab);
            instance.OnCloseClicked += HandleClose;
        }

        public override void Tick()
        {
        }

        public override void Exit()
        {
            if (instance != null)
            {
                instance.OnCloseClicked -= HandleClose;

                GameStateManager.Instance.DestroyUI(instance);
                instance = null;
            }
        }

        public void Tick(float deltaTime)
        {
            if (InputUtils.WasPausePressedThisFrame())
            {
                HandleClose();
            }
        }

        private void HandleClose()
        {
            GameStateManager.Instance.PopState();

            if (GameStateManager.Instance.CurrentState is PauseState pause)
            {
                pause.ShowUI();
            }
        }
    }

    #endregion

    #region BossState

    public class BossState : State
    {
        private enum BossPhase
        {
            IntroCutscene,
            Fight,
            DeathCutscene
        }

        private BossPhase phase;
        private int round;

        public override GameState GameState => GameState.BossFight;

        public BossState(int roundNumber)
        {
            round = roundNumber;
        }

        public override void Enter()
        {
            phase = BossPhase.IntroCutscene;
            Debug.Log($"Boss Round {round} - Intro Cutscene");
            GameStateManager.Instance.StartManagedCoroutine(IntroRoutine());
        }

        public override void Tick()
        {
        }

        public override void Exit()
        {
            // If Boss had UI, destroy it here. For now, just log exit.
            Debug.Log("Exiting Boss State");
        }

        public void Tick(float deltaTime)
        {
            // Boss fight logic can go here
        }

        private System.Collections.IEnumerator IntroRoutine()
        {
            yield return new WaitForSecondsRealtime(2f);
            phase = BossPhase.Fight;
            StartFight();

            yield return new WaitForSecondsRealtime(5f);
            phase = BossPhase.DeathCutscene;

            yield return PlayDeathCutsceneRoutine();

            GameStateManager.Instance.PopState();
        }

        private void StartFight()
        {
            Debug.Log("Boss Fight begins!");
        }

        private System.Collections.IEnumerator PlayDeathCutsceneRoutine()
        {
            Debug.Log("Boss Death Cutscene");
            yield return new WaitForSecondsRealtime(2f);
        }
    }

    #endregion
}