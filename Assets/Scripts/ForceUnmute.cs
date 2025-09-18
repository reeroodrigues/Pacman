using UnityEngine;

public class ForceUnmute : MonoBehaviour
{
    private void Awake()
    {
        if (AudioManager.I == null)
        {
            var audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                var go = new GameObject("AudioManager_Temp");
                audioManager = go.AddComponent<AudioManager>();
            }
        }
        
        PlayerPrefs.SetInt("am_muted", 0);
        AudioListener.volume = 1f;
        if (AudioManager.I != null)
        {
            AudioManager.I.SetMute(false);
        }
    }
}