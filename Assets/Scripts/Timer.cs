using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public enum TimerMode { CountDown, CountUp }
    public enum StartBehavior { Manual, AutoStart }
    public enum DisplayFormat { MM_SS, MM_SS_ms, SS, SS_ms }

    [Header("Configuração")]
    [SerializeField] private TimerMode mode = TimerMode.CountDown;
    [SerializeField] private StartBehavior startBehavior = StartBehavior.AutoStart;
    [SerializeField] private bool useUnscaledTime = false;
    [SerializeField] private float speedMultiplier = 1f;

    [Tooltip("Tempo total quando em CountDown; em CountUp é o limite opcional (0 = sem limite).")]
    [Min(0)][SerializeField] private float totalSeconds = 60f;

    [Header("UI (opcional)")]
#if TMP_PRESENT || TEXTMESHPRO || UNITY_TEXTMESHPRO
    [SerializeField] private TMP_Text tmpText;
#endif
    [SerializeField] private Text uiText;
    [SerializeField] private DisplayFormat displayFormat = DisplayFormat.SS;
    [SerializeField] private bool updateEveryFrame = true;

    [Header("Contador Visual")]
    [SerializeField] private Image timerVisual;
    [SerializeField] private bool clockwiseDecrease = true; 

    [Header("Eventos")]
    public UnityEvent onStarted;
    public UnityEvent onPaused;
    public UnityEvent onResumed;
    public UnityEvent onCompleted;
    public UnityEvent<int> onSecondTick;

    // Estado
    public bool IsRunning { get; private set; }
    public float CurrentSeconds => current;

    private float current;
    private int lastWholeSecond = int.MinValue;

    private void Awake()
    {
        ResetTimer(hardReset: true);
        UpdateLabel(force: true);
        if (timerVisual) InitializeVisualTimer();
    }

    private void Start()
    {
        if (startBehavior == StartBehavior.AutoStart)
            StartTimer();
    }

    private void Update()
    {
        if (!IsRunning) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        dt *= Mathf.Max(0f, speedMultiplier);

        if (mode == TimerMode.CountDown)
        {
            current -= dt;
            if (current <= 0f)
            {
                current = 0f;
                StopTimer(invokeComplete: true);
            }
        }
        else
        {
            current += dt;
            if (totalSeconds > 0f && current >= totalSeconds)
            {
                current = totalSeconds;
                StopTimer(invokeComplete: true);
            }
        }

        int whole = Mathf.FloorToInt(current);
        if (whole != lastWholeSecond)
        {
            lastWholeSecond = whole;
            onSecondTick?.Invoke(whole);
            if (!updateEveryFrame) UpdateLabel();
        }

        if (updateEveryFrame)
        {
            UpdateLabel();
            if (timerVisual) UpdateVisualTimer();
        }
    }

    public void StartTimer()
    {
        if (IsRunning) return;
        IsRunning = true;
        onStarted?.Invoke();
    }

    public void PauseTimer()
    {
        if (!IsRunning) return;
        IsRunning = false;
        onPaused?.Invoke();
    }

    public void ResumeTimer()
    {
        if (IsRunning) return;
        IsRunning = true;
        onResumed?.Invoke();
    }

    /// <summary>Reinicia o tempo. Em CountDown volta para totalSeconds; em CountUp volta para 0.</summary>
    public void ResetTimer(bool hardReset = false)
    {
        if (mode == TimerMode.CountDown)
            current = Mathf.Max(0f, totalSeconds);
        else
            current = 0f;

        lastWholeSecond = int.MinValue;

        if (hardReset) IsRunning = false;
        UpdateLabel(force: true);
        if (timerVisual) InitializeVisualTimer();
    }

    public void StopTimer(bool invokeComplete = false)
    {
        IsRunning = false;
        if (invokeComplete) onCompleted?.Invoke();
        if (timerVisual) UpdateVisualTimer(); 
    }

    /// <summary>Altera o total/límite em runtime (ajusta o valor atual de forma segura).</summary>
    public void SetTotalSeconds(float seconds, bool keepRatio = false)
    {
        seconds = Mathf.Max(0f, seconds);
        float ratio = (mode == TimerMode.CountDown && totalSeconds > 0f) ? current / totalSeconds : 0f;
        totalSeconds = seconds;

        if (mode == TimerMode.CountDown)
            current = keepRatio && totalSeconds > 0f ? totalSeconds * ratio : Mathf.Min(current, totalSeconds);
        else if (totalSeconds > 0f)
            current = Mathf.Min(current, totalSeconds);

        UpdateLabel(force: true);
        if (timerVisual) InitializeVisualTimer();
    }

    public void AddTime(float seconds)
    {
        if (seconds <= 0f) return;
        if (mode == TimerMode.CountDown)
            current = Mathf.Min(current + seconds, totalSeconds > 0 ? totalSeconds : current + seconds);
        else
            current += seconds;

        UpdateLabel();
        if (timerVisual) UpdateVisualTimer();
    }

    public void RemoveTime(float seconds)
    {
        if (seconds <= 0f) return;
        if (mode == TimerMode.CountDown)
            current = Mathf.Max(0f, current - seconds);
        else
            current = Mathf.Max(0f, current - seconds);

        UpdateLabel();
        if (timerVisual) UpdateVisualTimer();
    }

    private void UpdateLabel(bool force = false)
    {
        if (!force && !updateEveryFrame && Mathf.FloorToInt(current) == lastWholeSecond)
            return;

        string s = FormatTime(current, mode, totalSeconds, displayFormat);
#if TMP_PRESENT || TEXTMESHPRO || UNITY_TEXTMESHPRO
        if (tmpText) tmpText.text = s;
#endif
        if (uiText) uiText.text = s;
    }

    private void InitializeVisualTimer()
    {
        if (timerVisual && timerVisual.sprite)
        {
            timerVisual.fillMethod = Image.FillMethod.Radial360;
            timerVisual.type = Image.Type.Filled;
        }
    }

    private void UpdateVisualTimer()
    {
        if (timerVisual && timerVisual.sprite)
        {
            float fillAmount = mode == TimerMode.CountDown ? current / totalSeconds : current / totalSeconds;
            timerVisual.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }

    private static string FormatTime(float t, TimerMode mode, float limit, DisplayFormat fmt)
    {
        float show = Mathf.Max(0f, t);
        int minutes = Mathf.FloorToInt(show / 60f);
        int seconds = Mathf.FloorToInt(show % 60f);
        int ms = Mathf.FloorToInt((show - Mathf.Floor(show)) * 1000f);

        switch (fmt)
        {
            case DisplayFormat.MM_SS_ms:
                return $"{minutes:00}:{seconds:00}.{ms / 10:00}";
            case DisplayFormat.SS:
                return $"{Mathf.FloorToInt(show):00}";
            case DisplayFormat.SS_ms:
                return $"{Mathf.FloorToInt(show):00}.{ms / 10:00}";
            case DisplayFormat.MM_SS:
            default:
                return $"{minutes:00}:{seconds:00}";
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        totalSeconds = Mathf.Max(0f, totalSeconds);
        speedMultiplier = Mathf.Max(0f, speedMultiplier);
        if (!Application.isPlaying) UpdateLabel(force: true);
    }
#endif
}