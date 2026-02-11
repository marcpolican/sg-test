using UnityEngine;
using UnityEngine.SceneManagement;

public class UIBase : MonoBehaviour
{
    private void Awake()
    {
        UIFPS.Initialize();
    }

    public void OnButtonSceneChange(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
