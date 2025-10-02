using UnityEngine;

#region MenuState
public class MenuState : IGameState, IMenuState
{
    private MenuUI prefab;
    private MenuUI instance;

    public MenuState(MenuUI prefab)
    {
        this.prefab = prefab;
    }

    public void Enter()
    {
        instance = GameStateManager.Instance.SpawnUI(prefab);
        instance.onStartClicked += HandleStart;
        instance.onOptionsClicked += HandleOptions;
        instance.onQuitClicked += HandleQuit;
    }

    public void Exit()
    {
        if (instance != null)
        {
            instance.onStartClicked -= HandleStart;
            instance.onOptionsClicked -= HandleOptions;
            instance.onQuitClicked -= HandleQuit;

            GameStateManager.Instance.DestroyUI(instance);
            instance = null;
        }
    }

    public void Tick(float deltaTime) { }

    private void HandleStart()
    {
        GameStateManager.Instance.ChangeState(new PlayingState(GameStateManager.Instance.PlayingUIPrefab));
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
public class PlayingState : IGameState, IPlayingState
{
    private PlayingUI prefab;
    private PlayingUI instance;

    private int round = 1;
    private int maxAmmo;
    private int currentAmmo;
    private AmmoCounterUI ammoUI;

    public PlayingState(PlayingUI prefab)
    {
        this.prefab = prefab;
    }

    public void Enter()
    {
        instance = GameStateManager.Instance.SpawnUI(prefab);

        ammoUI = instance.GetComponentInChildren<AmmoCounterUI>();
        NozzleController nozzle = GameObject.FindFirstObjectByType<NozzleController>();
        maxAmmo = nozzle != null ? nozzle.maxAmmo : 6;
        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        Debug.Log($"Playing - Round {round}");
    }

    public void Exit()
    {
        GameStateManager.Instance.DestroyUI(instance);
        instance = null;
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
public class PauseState : IGameState
{
    private PauseUI prefab;
    private PauseUI instance;

    public PauseState(PauseUI prefab)
    {
        this.prefab = prefab;
    }

    public void Enter()
    {
        instance = GameStateManager.Instance.SpawnUI(prefab);
        instance.onResumeClicked += Resume;
        instance.onOptionsClicked += OpenOptions;
        instance.onExitToMenuClicked += ExitToMenu;
    }

    public void Exit()
    {
        if (instance != null)
        {
            instance.onResumeClicked -= Resume;
            instance.onOptionsClicked -= OpenOptions;
            instance.onExitToMenuClicked -= ExitToMenu;

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
public class OptionsState : IGameState
{
    private OptionsUI prefab;
    private OptionsUI instance;

    public OptionsState(OptionsUI prefab)
    {
        this.prefab = prefab;
    }

    public void Enter()
    {
        instance = GameStateManager.Instance.SpawnUI(prefab);
        instance.onCloseClicked += HandleClose;
    }

    public void Exit()
    {
        if (instance != null)
        {
            instance.onCloseClicked -= HandleClose;

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
public class BossState : IGameState
{
    private enum BossPhase { IntroCutscene, Fight, DeathCutscene }
    private BossPhase phase;
    private int round;

    public BossState(int roundNumber)
    {
        round = roundNumber;
    }

    public void Enter()
    {
        phase = BossPhase.IntroCutscene;
        Debug.Log($"Boss Round {round} - Intro Cutscene");
        GameStateManager.Instance.StartManagedCoroutine(IntroRoutine());
    }

    public void Exit()
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
