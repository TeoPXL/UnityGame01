// lightweight marker interfaces to mark categories of states
public interface IPausableState {
    void ShowUI();  // show the UI without recreating
    void HideUI();  // hide the UI without destroying
 }   // used for Pause-like states
public interface IPlayingState { }   // optional: states considered "playing"
public interface IMenuState { }      // optional: menu states
public interface IOptionsState { }   // optional: options/modal states
