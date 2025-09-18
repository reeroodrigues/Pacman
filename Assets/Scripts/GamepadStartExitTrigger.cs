#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamepadStartExitTrigger : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ExitGameButton exitButton;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Selectable defaultFocus;
    [SerializeField] private bool alsoOnEscape = false;

    [Header("Audio Toggle (Share/Select)")]
    [SerializeField] private bool enableAudioToggle = true;
    [SerializeField] private bool acceptTouchpadClick = true;

    private static bool sMuted;
    private const string K_MUTE = "mute";

    void Reset()
    {
        if (!exitButton) exitButton = FindObjectOfType<ExitGameButton>(true);
    }

    private void Awake()
    {
        sMuted = PlayerPrefs.GetInt(K_MUTE, 0) == 1;
        ApplyAudio();
    }

    void Update()
    {
        if (enableAudioToggle)
        {
#if ENABLE_INPUT_SYSTEM
            var pad = Gamepad.current;
            if (pad != null)
            {
                if (pad.selectButton.wasPressedThisFrame) ToggleAudio();
                
                if (acceptTouchpadClick && pad is DualShockGamepad ds && ds.touchpadButton.wasPressedThisFrame)
                    ToggleAudio();
            }
#else
            // Legacy Input: Select costuma ser o botão 6
            if (Input.GetKeyDown(KeyCode.JoystickButton6)) ToggleAudio();
#endif
        }
        
#if ENABLE_INPUT_SYSTEM
        var pad2 = Gamepad.current;
        if (pad2 != null && pad2.startButton.wasPressedThisFrame) OpenPanel();
#else
        if (Input.GetKeyDown(KeyCode.JoystickButton7)) OpenPanel(); // Start (legacy)
#endif
        if (alsoOnEscape && Input.GetKeyDown(KeyCode.Escape)) OpenPanel();
        
        if (confirmPanel != null && confirmPanel.activeSelf && exitButton != null)
        {
#if ENABLE_INPUT_SYSTEM
            var gp = Gamepad.current;
            if (gp != null)
            {
                if (gp.buttonSouth.wasPressedThisFrame) exitButton.ConfirmYes();
                if (gp.buttonEast.wasPressedThisFrame)  exitButton.ConfirmNo();
            }

            var ds = DualShockGamepad.current;
            if (ds != null)
            {
                if (ds.crossButton.wasPressedThisFrame)  exitButton.ConfirmYes();
                if (ds.circleButton.wasPressedThisFrame) exitButton.ConfirmNo();
            }
#else
            if (Input.GetKeyDown(KeyCode.JoystickButton0)) exitButton.ConfirmYes();
            if (Input.GetKeyDown(KeyCode.JoystickButton1)) exitButton.ConfirmNo();
            if (Input.GetKeyDown(KeyCode.JoystickButton2)) exitButton.ConfirmNo();
#endif
        }
    }

    private void ToggleAudio()
    {
        sMuted = !sMuted;
        PlayerPrefs.SetInt(K_MUTE, sMuted ? 1 : 0);
        ApplyAudio();
    }

    private void ApplyAudio()
    {
        AudioListener.volume = sMuted ? 0f : 1f;
    }

    private void OpenPanel()
    {
        if (exitButton == null) return;
        exitButton.OnExitClicked();

        if (confirmPanel != null && defaultFocus != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(defaultFocus.gameObject);
    }
}
