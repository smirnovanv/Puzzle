using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class StartState : IGameState
{
    private StateManager _stateManager;

    public SceneChanger SceneChanger { get; set; }

    public void EnterState(StateManager stateManager)
    {
        _stateManager = stateManager;
        Debug.Log("Bootstrap State Entered");
        // await InitializeServices();
        ExecuteState();
        StartMainScene();
    }

    //private async UniTask InitializeServices()
    //{
    //    Debug.Log("3");
    //    await UniTask.Delay(333);
    //    Debug.Log("2");
    //    await UniTask.Delay(333);
    //    Debug.Log("1");
    //    await UniTask.Delay(333);
    //    Debug.Log("Start");
    //    ExecuteState();
    //}

    public void ExecuteState()
    {
        Debug.Log("Bootstrap ExecuteState");
    }

    public void ExitState()
    {
        Debug.Log("Bootstrap State Exited");
    }

    private void StartMainScene()
    {
        _stateManager.StartState(new MainState());
    }
}
