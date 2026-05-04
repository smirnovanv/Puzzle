using System;
using Zenject;

public class StateManager
{
    private IGameState _previousState;
    private IGameState _currentState;
    private SceneChanger _sceneChanger;

    [Inject]
    public void Construct(SceneChanger sceneChanger)
    {
        _sceneChanger = sceneChanger;
    }

    public void StartState(IGameState newState)
    {
        _currentState?.ExitState();
        _previousState = _currentState;
        _currentState = newState;
        _currentState.SceneChanger = _sceneChanger;
        _currentState.EnterState(this);
    }

    public Type GetCurrentState()
    {
        return _currentState.GetType();
    }
}
