using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Passage : MonoBehaviour
{
    [Header("Ligação")]
    public Transform connection;

    [Header("Comportamento")]
    public bool onlyPacman = true;
    public float pushInside = 0.6f;
    public float cooldown = 0.1f;

    [Tooltip("Trava o eixo do movimento por um instante ao sair do túnel (evita virar para cima/baixo).")]
    public float axisLockSeconds = 0.25f;

    static readonly Dictionary<int, float> lastTeleportAt = new Dictionary<int, float>();
    Collider2D col;

    void Reset()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!connection) return;
        if (onlyPacman && other.GetComponent<Pacman>() == null) return;

        int id = other.GetInstanceID();
        float tNow = Time.unscaledTime;
        if (lastTeleportAt.TryGetValue(id, out float tPrev) && (tNow - tPrev) < cooldown)
            return;
        
        Vector2 entryDir = Vector2.zero;
        var mv = other.GetComponent<Movement>();
        if (mv != null) entryDir = mv.direction;
        
        Vector3 dst = connection.position;
        dst.z = other.transform.position.z;

        Vector2 inwardDir = (transform.position - connection.position).normalized;
        if (inwardDir.sqrMagnitude > 0.001f)
            dst += (Vector3)(inwardDir * pushInside);

        var rb = other.attachedRigidbody;
        if (rb != null) rb.position = dst; else other.transform.position = dst;
        
        if (mv != null && entryDir != Vector2.zero)
        {
            mv.SetDirection(entryDir, true);
            mv.ClearQueuedDirection();
            bool isHorizontal = Mathf.Abs(transform.position.x - connection.position.x) >=
                                Mathf.Abs(transform.position.y - connection.position.y);
            mv.LockAxisFor(isHorizontal ? Movement.Axis.Horizontal : Movement.Axis.Vertical, 0.25f);
        }
        
        if (mv != null && axisLockSeconds > 0f)
        {
            bool tunnelIsHorizontal = Mathf.Abs(transform.position.x - connection.position.x) >=
                                      Mathf.Abs(transform.position.y - connection.position.y);
            var axis = tunnelIsHorizontal ? Movement.Axis.Horizontal : Movement.Axis.Vertical;
            mv.LockAxisFor(axis, axisLockSeconds);
        }

        lastTeleportAt[id] = tNow;
    }
}
