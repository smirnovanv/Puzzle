using UnityEngine;

public class MainState : IGameState
{
    private StateManager _stateManager;

    public SceneChanger SceneChanger { get; set; }

    public void EnterState(StateManager stateManager)
    {
        Debug.Log("MainScene State Entered");
        _stateManager = stateManager;
        ExecuteState();
    }

    public void ExecuteState()
    {
        SceneChanger.ChangeScene((int)ScenesDictionary.MainScene);
    }

    public void ExitState()
    {
        Debug.Log("MainScene State Exited");
    }

}
