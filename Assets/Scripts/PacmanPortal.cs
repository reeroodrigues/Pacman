using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TunnelPortal : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Vector2 pushDirection;
    [SerializeField] private float pushDistance = 0.6f;
    [SerializeField] private float cooldown = 0.2f;
    
    private static readonly Dictionary<Collider2D, float> _nextAllowed = new();

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        float tNow = Time.time;
        if (_nextAllowed.TryGetValue(other, out float until) && tNow < until)
            return;
        
        Vector3 outPos = exitPoint.position + (Vector3)(pushDirection.normalized * pushDistance);
        other.transform.position = outPos;
        
        if (other.TryGetComponent<Rigidbody2D>(out var rb))
        {
            float speed = rb.linearVelocity.magnitude;
            if (speed <= 0.01f) speed = 5f;
            rb.linearVelocity = pushDirection.normalized * speed;
        }
        
        if (other.TryGetComponent<PacmanMovement>(out var move))
        {
            move.SetDesiredDirection(pushDirection.normalized);
            move.ForceCurrentDirection(pushDirection.normalized);
        }
        _nextAllowed[other] = tNow + cooldown;
    }
}