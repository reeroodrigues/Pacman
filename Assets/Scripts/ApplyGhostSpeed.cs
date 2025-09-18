using UnityEngine;

[RequireComponent(typeof(Movement))]
public class ApplyGhostSpeed : MonoBehaviour
{
    private Movement _movement;
    private int _lastApplied = int.MinValue;

    private void Awake()
    {
        _movement = GetComponent<Movement>();
        var fallback = Mathf.RoundToInt(Mathf.Clamp(_movement.speed, 0f, 10f));
        GhostSpeedSettings.EnsureDefault(fallback);
        
        ApplyNow(true);
    }

    private void OnEnable() => ApplyNow(true);
    private void Update() => ApplyNow(false);

    private void ApplyNow(bool force)
    {
        var target = GhostSpeedSettings.Speed;
        if (force || target != _lastApplied)
        {
            _movement.speed = target;
            _lastApplied = target;
        }
    }
}