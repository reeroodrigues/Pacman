using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("UI (opcional)")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool autoStart = true;

    private bool _running;
    private float _elapsed;

    private void OnEnable()
    {
        GameEvents.AllPelletsCollected += StopTimer;
        GameEvents.PacmanDied         += StopTimer;
    }
    private void OnDisable()
    {
        GameEvents.AllPelletsCollected -= StopTimer;
        GameEvents.PacmanDied          -= StopTimer;
    }

    private void Start()
    {
        ResetTimer();
        if (autoStart) StartTimer();
    }

    private void Update()
    {
        if (!_running) return;
        _elapsed += Time.deltaTime;
        if (timerText) timerText.text = FormatTime(_elapsed);
    }

    public void StartTimer() => _running = true;
    public void StopTimer()  => _running = false;
    public void ResetTimer()
    {
        _elapsed = 0f;
        if (timerText) timerText.text = "00:00";
    }

    public float ElapsedSeconds => _elapsed;

    private string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}