using UnityEngine;
using UnityEngine.SceneManagement;

public class UIBase : MonoBehaviour
{
    public void OnButtonSceneChange(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
