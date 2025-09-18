using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitGameButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private bool confirmBeforeExit = true;

    [Header("Comportamento")]
    [SerializeField] private bool pauseOnOpen = true;
    [SerializeField] private bool resumeOnClose = true;
    [SerializeField] private string menuSceneName = "MainMenu"; // nome exato da sua cena de menu
    [SerializeField] private bool autoQuitWhenOnMenu = true;    // se estiver no menu, fecha app

    private float _prevTimeScale = 1f;
    private bool _pausedByMe = false;

    private void Awake()
    {
        if (exitButton) exitButton.onClick.AddListener(OnExitClicked);
        if (confirmPanel) confirmPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (exitButton) exitButton.onClick.RemoveListener(OnExitClicked);
    }

    public void OnExitClicked()
    {
        if (confirmBeforeExit && confirmPanel != null)
        {
            OpenConfirmPanel();
        }
        else
        {
            ExecuteExitAction();
        }
    }

    private void OpenConfirmPanel()
    {
        if (!confirmPanel) { ExecuteExitAction(); return; }

        confirmPanel.SetActive(true);

        if (pauseOnOpen && !_pausedByMe)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _pausedByMe = true;
        }
    }

    private void CloseConfirmPanel()
    {
        if (confirmPanel) confirmPanel.SetActive(false);

        if (_pausedByMe && resumeOnClose)
        {
            Time.timeScale = _prevTimeScale;
            _pausedByMe = false;
        }
    }
    
    // Botão "Sim" do painel
    public void ConfirmYes()
    {
        if (_pausedByMe) { Time.timeScale = _prevTimeScale; _pausedByMe = false; }
        if (confirmPanel) confirmPanel.SetActive(false);
        ExecuteExitAction();
    }

    // Botão "Não" do painel
    public void ConfirmNo()
    {
        CloseConfirmPanel();
    }

    private void ExecuteExitAction()
    {
        // garantir que a próxima cena (se houver) não fique pausada
        Time.timeScale = 1f;

        bool onMenu = autoQuitWhenOnMenu &&
                      SceneManager.GetActiveScene().name == menuSceneName;

        if (onMenu)
        {
            QuitApp();
        }
        else
        {
            // voltar pro menu a partir de qualquer outra cena
            StartCoroutine(LoadMenuNextFrame());
        }
    }

    private System.Collections.IEnumerator LoadMenuNextFrame()
    {
        yield return null; // evita conflito com UI/click do mesmo frame
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }

    private void QuitApp()
    {
        // Fecha o app (no Editor, para o Play)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                activity.Call("finishAndRemoveTask");
            }
        }
        catch
        {
            Application.Quit();
        }
#else
        Application.Quit();
#endif
    }
}
