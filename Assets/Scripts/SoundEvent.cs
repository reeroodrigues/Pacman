using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SND_New", menuName = "Audio/Sound Event")]
public class SoundEvent : ScriptableObject
{
    [Header("Clips")]
    public AudioClip[] clips;

    [Header("Mixer")]
    public AudioMixerGroup outputGroup;

    [Header("Ganho e Pitch")]
#if UNITY_EDITOR
    [MinMaxRange(0.5f, 2f)]
#endif
    public Vector2 pitchRange = new Vector2(1f, 1f);
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Loop")]
    public bool loop = false;

    public AudioClip PickClip()
    {
        if (clips == null || clips.Length == 0)
            return null;

        if (clips.Length == 1)
            return clips[0];

        var i = Random.Range(0, clips.Length);
        return clips[i];
    }

    public float PickPitch() => Random.Range(pitchRange.x, pitchRange.y);
}