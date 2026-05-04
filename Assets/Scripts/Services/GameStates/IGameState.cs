public interface IGameState
{
    public SceneChanger SceneChanger { get; set; }
    public void EnterState(StateManager stateManager);
    public void ExecuteState();
    public void ExitState();
}
