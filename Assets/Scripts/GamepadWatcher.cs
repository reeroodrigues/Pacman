using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadWatcher : MonoBehaviour
{
    [SerializeField] private CanvasGroup dpadCanvas;

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        ApplyState();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
            ApplyState();
    }

    private void ApplyState()
    {
        bool hasGamepad = Gamepad.current != null;
        if (!dpadCanvas) return;

        dpadCanvas.alpha = hasGamepad ? 0f : 1f;
        dpadCanvas.interactable = !hasGamepad;
        dpadCanvas.blocksRaycasts = !hasGamepad;
    }
}