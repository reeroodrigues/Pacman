using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TunnelAxisLockZone : MonoBehaviour
{
    public bool onlyPacman = true;
    public bool horizontal = true;
    public float lockRefresh = 0.05f;
    public float lockWindow  = 0.12f;

    private float _lastApply;

    private void Reset() { GetComponent<Collider2D>().isTrigger = true; }
    private void OnValidate() { GetComponent<Collider2D>().isTrigger = true; }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (onlyPacman && other.GetComponent<Pacman>() == null) return;
        var mv = other.GetComponent<Movement>(); if (!mv) return;

        if (Time.unscaledTime - _lastApply >= lockRefresh)
        {
            mv.LockAxisFor(horizontal ? Movement.Axis.Horizontal : Movement.Axis.Vertical, lockWindow);
            _lastApply = Time.unscaledTime;
        }
    }
}