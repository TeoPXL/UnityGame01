public interface IGameState
{
    /// Called when the state becomes active.
    void Enter();

    /// Called when the state is popped/changed away from.
    void Exit();

    /// Called every frame while this state is the top of the stack.
    void Tick(float deltaTime);
}
