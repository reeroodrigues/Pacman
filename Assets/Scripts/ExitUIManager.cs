using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
#endif

public class ExitUIManager : MonoBehaviour
{
    public static ExitUIManager Instance { get; private set; }

    [Header("Overlay")]
    [SerializeField] private CanvasGroup container;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Selectable defaultFocus;

    [Header("Comportamento")]
    [SerializeField] private bool pauseOnOpen = true;
    [SerializeField] private bool resumeOnClose = true;
    [SerializeField] private bool backOpensWhenHidden = false;

    private float _prevTimeScale = 1f;
    private bool _pausedByMe = false;

    public bool PanelActive => confirmPanel != null && confirmPanel.activeSelf;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (container != null) { container.alpha = 0f; container.blocksRaycasts = false; container.interactable = false; }
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var pad = Gamepad.current;
        if (pad != null && pad.startButton.wasPressedThisFrame) Open();
#else
        if (Input.GetKeyDown(KeyCode.JoystickButton7)) Open();
#endif
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PanelActive) ConfirmNo();
            else if (backOpensWhenHidden) Open();
        }
        
        if (PanelActive)
        {
#if ENABLE_INPUT_SYSTEM
            var gp = Gamepad.current;
            if (gp != null)
            {
                if (gp.buttonSouth.wasPressedThisFrame) ConfirmYes();
                if (gp.buttonEast.wasPressedThisFrame)  ConfirmNo();
            }
            var ds = DualShockGamepad.current;
            if (ds != null)
            {
                if (ds.crossButton.wasPressedThisFrame)  ConfirmYes();
                if (ds.circleButton.wasPressedThisFrame) ConfirmNo();
            }
#else
            if (Input.GetKeyDown(KeyCode.JoystickButton0)) ConfirmYes(); // A / X
            if (Input.GetKeyDown(KeyCode.JoystickButton1)) ConfirmNo();  // B / O
            if (Input.GetKeyDown(KeyCode.JoystickButton2)) ConfirmNo();  // (alguns drivers mapeiam O aqui)
#endif
        }
    }
    
    public void Open()
    {
        if (confirmPanel == null || container == null) return;

        confirmPanel.SetActive(true);
        container.alpha = 1f;
        container.blocksRaycasts = true;
        container.interactable = true;

        if (pauseOnOpen && !_pausedByMe)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _pausedByMe = true;
        }

        if (defaultFocus != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(defaultFocus.gameObject);
    }

    public void ConfirmNo()  => Close();
    public void ConfirmYes() => QuitNow();

    public void Close()
    {
        if (confirmPanel == null || container == null) return;

        confirmPanel.SetActive(false);
        container.alpha = 0f;
        container.blocksRaycasts = false;
        container.interactable = false;

        if (_pausedByMe && resumeOnClose)
        {
            Time.timeScale = _prevTimeScale;
            _pausedByMe = false;
        }
    }

    private void QuitNow()
    {
        if (_pausedByMe) { Time.timeScale = _prevTimeScale; _pausedByMe = false; }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; return;
#endif
#if UNITY_ANDROID
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                activity.Call("finishAndRemoveTask");
        }
        catch { Application.Quit(); }
#else
        Application.Quit();
#endif
    }
}
