using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneChanger
{
    public bool ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        return true;
    }


}
