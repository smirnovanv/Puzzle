using UnityEngine;
using Zenject;
public class GameInitializer : IInitializable
{
    private StateManager _stateManager;

    [Inject]
    public GameInitializer(StateManager stateManager)
    {
        Debug.Log("GameInitializer constructed");
        _stateManager = stateManager;
    }

    public void Initialize()
    {
        Debug.Log("GameInitializer.Initialize() called!");
        _stateManager.StartState(new StartState());
    }
}
