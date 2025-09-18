using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private string mainMenuScene = "";

    public void OnBack()
    {
        if (!string.IsNullOrEmpty(mainMenuScene))
            SceneManager.LoadScene(mainMenuScene);
        else
            SceneManager.LoadScene(0);
    }

    public void OnRestart()
    {
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}