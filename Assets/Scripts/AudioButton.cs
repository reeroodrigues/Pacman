using UnityEngine;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour
{
    [SerializeField] private Button button;
    private static bool _muted;

    private void Awake() {
        if (!button) button = GetComponent<Button>();
        if (button) button.onClick.AddListener(ToggleAudio);
        _muted = PlayerPrefs.GetInt("mute", 0) == 1;
        Apply();
    }

    private void ToggleAudio() {
        _muted = !_muted;
        PlayerPrefs.SetInt("mute", _muted ? 1 : 0);
        Apply();
    }

    private void Apply()
    {
        AudioListener.volume = _muted ? 0f : 1f;
    }
}