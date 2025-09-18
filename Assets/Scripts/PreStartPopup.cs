using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if TMP_PRESENT || TEXTMESHPRO || UNITY_TEXTMESHPRO
#endif

public class PreStartPopup : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup container;
#if TMP_PRESENT || TEXTMESHPRO || UNITY_TEXTMESHPRO
    [SerializeField] private TextMeshProUGUI countdownTMP;  // opcional (TextMeshPro)
#endif
    [SerializeField] private TextMeshProUGUI countdownUI;
    [SerializeField] private Button backButton;

    [Header("Fluxo")]
    [SerializeField] private bool showOnAwake = true;
    [SerializeField] private int startFrom = 3;
    [SerializeField] private float stepSeconds = 1.0f;
    [SerializeField] private string finalText = "Start!";
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private bool pauseGameplay = true;

    [Header("Integração (opcional)")]
    [SerializeField] private Timer roundTimer;
    [SerializeField] private bool startTimerAfterCountdown = true;

    private bool running;

    private void Awake()
    {
        if (container != null)
        {
            container.alpha = 0f;
            container.blocksRaycasts = false;
            container.interactable = false;
        }

        if (backButton != null)
            backButton.onClick.AddListener(OnBackToMenu);

        if (showOnAwake)
            ShowAndBegin();
    }

    private void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackToMenu);
    }

    public void ShowAndBegin()
    {
        if (running) return;
        running = true;

        if (pauseGameplay) Time.timeScale = 0f;

        if (container != null)
        {
            container.alpha = 1f;
            container.blocksRaycasts = true;
            container.interactable = true;
        }

        StartCoroutine(CountdownRoutine());
    }

    private System.Collections.IEnumerator CountdownRoutine()
    {
        for (int n = startFrom; n >= 1; n--)
        {
            SetText(n.ToString());
            yield return WaitRealtime(stepSeconds);
        }
        
        SetText(finalText);
        yield return WaitRealtime(0.5f);
        
        if (container != null)
        {
            container.alpha = 0f;
            container.blocksRaycasts = false;
            container.interactable = false;
        }

        if (pauseGameplay) Time.timeScale = 1f;

        if (startTimerAfterCountdown && roundTimer != null)
            roundTimer.StartTimer();

        running = false;
    }

    private void SetText(string s)
    {
#if TMP_PRESENT || TEXTMESHPRO || UNITY_TEXTMESHPRO
        if (countdownTMP) countdownTMP.text = s;
#endif
        if (countdownUI) countdownUI.text = s;
    }

    private System.Collections.IEnumerator WaitRealtime(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void OnBackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnBackToMenu();

        #if ENABLE_INPUT_SYSTEM
        var pad = UnityEngine.InputSystem.Gamepad.current;
        if (pad != null && pad.buttonWest.wasPressedThisFrame)
            OnBackToMenu();
        #endif
    }
}
