using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MainSceneUI : MonoBehaviour
{
    [SerializeField] private Button startGameButton;

    private SceneChanger _sceneChanger;

    [Inject]
    public void Construct( SceneChanger sceneChanger )
    {
        _sceneChanger = sceneChanger;
    }

    void Start()
    {
        startGameButton.onClick?.AddListener(StartGame);

    }
    private void StartGame()
    {
        Debug.Log("StartGame");

        _sceneChanger.ChangeScene((int)ScenesDictionary.BaseScene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
