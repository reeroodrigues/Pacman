using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Áudio central do jogo:
/// - SFX: pool de AudioSources para tocar efeitos (2D ou 3D)
/// - Música: 2 trilhas com crossfade
/// - Volumes por AudioMixer ("MusicVol" e "SFXVol") + Mute com PlayerPrefs
/// Uso:
///   AudioManager.I.Play2D(someSfx);
///   AudioManager.I.PlayAt(someSfx, worldPos);
///   AudioManager.I.PlayMusic(musicEvent, loop:true);
///   AudioManager.I.StopMusic();
///   AudioManager.I.ToggleMute();
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Mixer e Grupos")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;

    [Header("Pool de SFX")]
    [SerializeField] private int sfxPoolSize = 12;

    [Tooltip("Distância mínima (3D)")]
    [SerializeField] private float sfxMinDistance = 1f;
    [Tooltip("Distância máxima (3D)")]
    [SerializeField] private float sfxMaxDistance = 20f;

    [Header("Música")]
    [SerializeField] private float defaultMusicFade = 0.5f;
    
    private readonly List<AudioSource> _sfxPool = new();
    private AudioSource _musicA, _musicB;
    
    private float _music01 = 1f, _sfx01 = 1f;
    private bool _muted = false;

    public bool IsMuted => _muted;
    
    public Action<bool> OnMuteChanged;
    
    private const string K_MUSIC = "am_music01";
    private const string K_SFX   = "am_sfx01";
    private const string K_MUTED = "am_muted";
    
    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        
        for (int i = 0; i < Mathf.Max(1, sfxPoolSize); i++)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = false;
            s.spatialBlend = 0f;
            s.outputAudioMixerGroup = sfxGroup;
            s.rolloffMode = AudioRolloffMode.Linear;
            s.minDistance = sfxMinDistance;
            s.maxDistance = sfxMaxDistance;
            s.dopplerLevel = 0f;
            _sfxPool.Add(s);
        }
        
        _musicA = gameObject.AddComponent<AudioSource>();
        _musicB = gameObject.AddComponent<AudioSource>();
        foreach (var m in new[] { _musicA, _musicB })
        {
            m.playOnAwake = false;
            m.loop = true;
            m.spatialBlend = 0f;
            m.outputAudioMixerGroup = musicGroup;
            m.volume = 0f;
        }
        
        _music01 = PlayerPrefs.GetFloat(K_MUSIC, 1f);
        _sfx01   = PlayerPrefs.GetFloat(K_SFX,   1f);
        _muted   = PlayerPrefs.GetInt(K_MUTED,   0) == 1;

        ApplyVolumesToMixer();
    }
    
    /// <summary>Toca SFX em 2D com variação de pitch do SoundEvent.</summary>
    public void Play2D(SoundEvent ev)
    {
        if (ev == null) return;
        var clip = ev.PickClip(); if (clip == null) return;

        var src = GetFreeSfxSource();
        SetupSfxSource(src, ev, is3D: false);
        src.PlayOneShot(clip, ev.volume);
    }

    /// <summary>Toca SFX posicionado no mundo (3D). Usa rolloff linear.</summary>
    public void PlayAt(SoundEvent ev, Vector3 worldPos)
    {
        if (ev == null) return;
        var clip = ev.PickClip(); if (clip == null) return;

        var src = GetFreeSfxSource();
        src.transform.position = worldPos;
        SetupSfxSource(src, ev, is3D: true);
        
        src.clip = clip;
        src.volume = ev.volume;
        src.loop = false;
        src.Play();
    }

    /// <summary>Para todos os SFX do pool imediatamente.</summary>
    public void StopAllSfx()
    {
        foreach (var s in _sfxPool) s.Stop();
    }
    
    /// <summary>Toca música com crossfade suave. Se ev.loop == true força loop.</summary>
    public void PlayMusic(SoundEvent ev, bool loop = true, float fade = -1f)
    {
        if (ev == null) return;
        var clip = ev.PickClip(); if (clip == null) return;

        var from = GetCurrentMusicSource();
        var to = from == _musicA ? _musicB : _musicA;
        
        to.Stop();
        to.clip = clip;
        to.loop = loop || ev.loop;
        to.outputAudioMixerGroup = ev.outputGroup != null ? ev.outputGroup : musicGroup;
        to.volume = 0f;
        to.Play();

        StartCoroutine(Crossfade(from, to, fade < 0f ? defaultMusicFade : fade));
    }

    /// <summary>Fade out e para qualquer música tocando.</summary>
    public void StopMusic(float fade = -1f)
    {
        var cur = GetCurrentMusicSource();
        if (cur == null || !cur.isPlaying) return;
        StartCoroutine(FadeOutAndStop(cur, fade < 0f ? defaultMusicFade : fade));
    }
    
    public void SetMusicVolume01(float value01)
    {
        _music01 = Mathf.Clamp01(value01);
        PlayerPrefs.SetFloat(K_MUSIC, _music01);
        if (!_muted && mixer != null) mixer.SetFloat("MusicVol", ToDb(_music01));
    }

    public void SetSfxVolume01(float value01)
    {
        _sfx01 = Mathf.Clamp01(value01);
        PlayerPrefs.SetFloat(K_SFX, _sfx01);
        if (!_muted && mixer != null) mixer.SetFloat("SFXVol", ToDb(_sfx01));
    }

    public void SetMute(bool mute)
    {
        if (_muted == mute) return;
        _muted = mute;
        PlayerPrefs.SetInt(K_MUTED, _muted ? 1 : 0);
        ApplyVolumesToMixer();
        OnMuteChanged?.Invoke(_muted);
    }

    public void ToggleMute() => SetMute(!_muted);

    public static float ToDb(float v01) => Mathf.Approximately(v01, 0f) ? -80f : Mathf.Log10(Mathf.Clamp01(v01)) * 20f;
    
    private void ApplyVolumesToMixer()
    {
        if (mixer == null) return;
        if (_muted)
        {
            mixer.SetFloat("MusicVol", ToDb(0f));
            mixer.SetFloat("SFXVol",   ToDb(0f));
        }
        else
        {
            mixer.SetFloat("MusicVol", ToDb(_music01));
            mixer.SetFloat("SFXVol",   ToDb(_sfx01));
        }
    }

    private AudioSource GetFreeSfxSource()
    {
        foreach (var s in _sfxPool)
            if (!s.isPlaying) return s;
        
        return _sfxPool[0];
    }

    private void SetupSfxSource(AudioSource src, SoundEvent ev, bool is3D)
    {
        src.outputAudioMixerGroup = ev.outputGroup != null ? ev.outputGroup : sfxGroup;
        src.pitch = ev.PickPitch();
        src.panStereo = 0f;
        src.reverbZoneMix = 1f;

        if (is3D)
        {
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = sfxMinDistance;
            src.maxDistance = sfxMaxDistance;
        }
        else
        {
            src.spatialBlend = 0f;
        }
    }

    private AudioSource GetCurrentMusicSource()
    {
        if (_musicA != null && _musicA.isPlaying) return _musicA;
        if (_musicB != null && _musicB.isPlaying) return _musicB;
        return _musicA;
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float time)
    {
        if (to == null) yield break;

        if (time <= 0f)
        {
            if (from != null && from.isPlaying) from.Stop();
            to.volume = 1f;
            yield break;
        }

        float t = 0f;
        float fromStart = from != null ? from.volume : 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / time);

            if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, k);
            to.volume = k;

            yield return null;
        }

        if (from != null && from.isPlaying) from.Stop();
        to.volume = 1f;
    }

    private IEnumerator FadeOutAndStop(AudioSource src, float time)
    {
        if (src == null) yield break;

        if (time <= 0f)
        {
            src.Stop();
            src.volume = 1f;
            yield break;
        }

        float start = src.volume;
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, 0f, t / time);
            yield return null;
        }
        src.Stop();
        src.volume = 1f;
    }
}
