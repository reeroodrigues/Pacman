using UnityEngine;
using UnityEngine.UI;

public class AudioOptionsUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        musicSlider.onValueChanged.AddListener(v => AudioManager.I.SetMusicVolume01(v));
        sfxSlider.onValueChanged.AddListener(v => AudioManager.I.SetSfxVolume01(v));
        
        AudioManager.I.SetMusicVolume01(musicSlider.value);
        AudioManager.I.SetSfxVolume01(sfxSlider.value);
    }
}