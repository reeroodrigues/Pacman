using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class PelletWakaController : MonoBehaviour
{
    public static PelletWakaController I { get; private set; }

    [Header("Eventos de Som")]
    [SerializeField] private SoundEvent pelletEvent;
    [SerializeField] private SoundEvent pelletLoopEvent;

    [Header("Mixer (fallback)")]
    [SerializeField] private AudioMixerGroup defaultGroup;

    [Header("Comportamento")]
    [SerializeField] private float eatInterval = 0.05f;
    [SerializeField] private float sustainTimeout = 0.35f;

    [Header("Pitch Ramp (opcional)")]
    [SerializeField] private float pitchStep = 0.02f;
    [SerializeField] private float minPitch = 1.00f;
    [SerializeField] private float maxPitch = 1.20f;

    private float _currentPitch;
    private float _lastEatTime = -999f;
    
    private AudioSource _loopSrc;
    
    private AudioSource _a, _b; bool _useA;
    private Coroutine _rhythmCo;
    private bool _running;

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        _currentPitch = minPitch;
        
        _loopSrc = gameObject.AddComponent<AudioSource>();
        _loopSrc.playOnAwake = false;
        _loopSrc.loop = true;
        _loopSrc.spatialBlend = 0f;
        
        _a = gameObject.AddComponent<AudioSource>();
        _b = gameObject.AddComponent<AudioSource>();
        foreach (var s in new[] { _a, _b })
        {
            s.playOnAwake = false; s.loop = false; s.spatialBlend = 0f;
        }
    }
    
    public void NotifyPelletEaten()
    {
        float now = Time.unscaledTime;
        
        _lastEatTime = now;
        
        _currentPitch = Mathf.Min(maxPitch, (now > 0 ? _currentPitch + pitchStep : minPitch));

        if (!_running)
        {
            _running = true;

            if (pelletLoopEvent != null && pelletLoopEvent.PickClip() != null)
            {
                StartLoop();
            }
            else
            {
                if (_rhythmCo != null) StopCoroutine(_rhythmCo);
                _rhythmCo = StartCoroutine(RhythmRoutine());
            }
        }
    }
    
    public void StopNow()
    {
        _running = false;
        if (_rhythmCo != null) { StopCoroutine(_rhythmCo); _rhythmCo = null; }
        if (_loopSrc.isPlaying) _loopSrc.Stop();
        _a.Stop(); _b.Stop();
        _currentPitch = minPitch;
        _lastEatTime = -999f;
    }

    public void ResetCombo() => StopNow();
    
    private void StartLoop()
    {
        if (pelletLoopEvent == null) return;
        var clip = pelletLoopEvent.PickClip(); if (clip == null) return;

        _loopSrc.outputAudioMixerGroup = pelletLoopEvent.outputGroup != null ? pelletLoopEvent.outputGroup : defaultGroup;
        _loopSrc.clip = clip;
        _loopSrc.pitch = _currentPitch;
        _loopSrc.volume = pelletLoopEvent.volume;
        _loopSrc.Play();

        if (_rhythmCo != null) { StopCoroutine(_rhythmCo); _rhythmCo = null; }
        _rhythmCo = StartCoroutine(LoopKeepAlive());
    }

    private IEnumerator LoopKeepAlive()
    {
        while (_running)
        {
            _loopSrc.pitch = _currentPitch;

            if (Time.unscaledTime - _lastEatTime > sustainTimeout)
            {
                _running = false;
                break;
            }
            yield return null;
        }
        _loopSrc.Stop();
    }
    
    private IEnumerator RhythmRoutine()
    {
        var group = (pelletEvent != null && pelletEvent.outputGroup != null) ? pelletEvent.outputGroup : defaultGroup;
        var clip = pelletEvent != null ? pelletEvent.PickClip() : null;

        if (clip == null)
        {
            _running = false;
            yield break;
        }

        double next = AudioSettings.dspTime + 0.001;

        while (_running)
        {
            if (Time.unscaledTime - _lastEatTime > sustainTimeout)
            {
                _running = false; break;
            }
            
            double now = AudioSettings.dspTime;
            if (now + 0.01 >= next)
            {
                var cur = _useA ? _a : _b;
                _useA = !_useA;

                cur.outputAudioMixerGroup = group;
                cur.clip = clip;
                cur.volume = pelletEvent.volume;
                cur.pitch = _currentPitch;

                cur.Stop();
                cur.PlayScheduled(next);

                next += eatInterval;
            }

            yield return null;
        }

        _a.Stop(); _b.Stop();
    }
}
