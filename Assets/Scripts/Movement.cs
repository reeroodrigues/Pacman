using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public float speed = 8f;
    public float speedMultiplier = 1f;
    public Vector2 initialDirection;
    public LayerMask obstacleLayer;
    private Axis _lockedAxis = Axis.None;
    private float _axisUnlockAt = 0f;
    // private Vector2 _lastRbPos;
    // private float _stuckTimer;
    
    [Header("Turn Assist / Colisão")]
    [SerializeField] private bool autoDetectTileSize = true;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float turnThreshold = 0.18f;
    [SerializeField] private Vector2 castBoxSize = new Vector2(0.6f, 0.6f);
    [SerializeField] private float castDistance = 1.0f;
    [SerializeField] private bool snapToCenterOnTurn = true;
    [SerializeField] private float axisLockDebugSeconds = 0f;
    // [SerializeField] private bool autoUnstuck = true;
    // [SerializeField] private float stuckTime = 0.3f;
    // [SerializeField] private float movedEps = 0.0005f;
    // [SerializeField] private bool allowReverseWhenStuck = true;
    
    [Header("Primeiro movimento")]
    [SerializeField] private bool horizontalFirstMoveOnly = true;
    private bool _firstMovePending = false;
    
    public Rigidbody2D rb { get; private set; }
    public Vector2 direction { get; private set; }
    public Vector2 nextDirection { get; private set; }
    public Vector3 startingPosition { get; private set; }
    public enum Axis { None, Horizontal, Vertical }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (autoDetectTileSize)
        {
            var g = GetComponentInParent<Grid>();
            if (g != null)
                tileSize = g.cellSize.x * g.transform.lossyScale.x;
        }
        //
        // _lastRbPos = rb.position;
        // _stuckTimer = 0f;
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        speedMultiplier = 1f;
        nextDirection = Vector2.zero;
        transform.position = startingPosition;
        rb.position = startingPosition;
        rb.isKinematic = false;
        enabled = true;

        if (horizontalFirstMoveOnly)
        {
            _firstMovePending = true;
            
            _lockedAxis = Axis.Horizontal;
            _axisUnlockAt = float.MaxValue;
            
            if (Mathf.Abs(initialDirection.x) >= Mathf.Abs(initialDirection.y))
                direction = initialDirection;
            else
                direction = Vector2.zero;
        }
        else
        {
            _firstMovePending = false;
            _lockedAxis = Axis.None;
            _axisUnlockAt = 0f;
            direction = initialDirection;
        }
    }

    private void Update()
    {
        if (nextDirection != Vector2.zero)
            SetDirection(nextDirection);
    }

    private void FixedUpdate()
    {
        if (nextDirection != Vector2.zero && !Occupied(nextDirection))
        {
            Vector2 pos = rb.position;
            Vector2 center = new Vector2(
                Mathf.Round(pos.x / tileSize) * tileSize,
                Mathf.Round(pos.y / tileSize) * tileSize
            );

            if ((pos - center).sqrMagnitude <= turnThreshold * turnThreshold)
            {
                if (snapToCenterOnTurn) rb.position = center;
                direction = nextDirection;
                nextDirection = Vector2.zero;
            }
        }
        
        Vector2 position = rb.position;
        Vector2 translation = speed * speedMultiplier * Time.fixedDeltaTime * direction;
        rb.MovePosition(position + translation);

        // bool moved = (rb.position - _lastRbPos).sqrMagnitude > (movedEps * movedEps);
        // if (moved)
        // {
        //     _stuckTimer = 0f;
        //     _lastRbPos = rb.position;
        // }
        // else if (autoUnstuck && direction != Vector2.zero && Occupied(direction))
        // {
        //     _stuckTimer += Time.fixedDeltaTime;
        //
        //     if (_stuckTimer >= stuckTime)
        //     {
        //         SnapToGridCenter();
        //
        //         if (!TryResolveStuck(trySmallerCast: true))
        //         {
        //             if (allowReverseWhenStuck)
        //             {
        //                 var rev = new Vector2(-direction.x, -direction.y);
        //                 if (!Occupied(rev))
        //                 {
        //                     direction = rev;
        //                     nextDirection = Vector2.zero;
        //                 }
        //             }
        //         }
        //
        //         _stuckTimer = 0f;
        //         _lastRbPos = rb.position;
        //     }
        // }
    }
    
    public void LockAxisFor(Axis axis, float seconds)
    {
        _lockedAxis = axis;
        _axisUnlockAt = Time.unscaledTime + Mathf.Max(0f, seconds);
    }
    
    private void LateUpdate()
    {
        if (_lockedAxis != Axis.None && Time.unscaledTime >= _axisUnlockAt)
            _lockedAxis = Axis.None;
        if (axisLockDebugSeconds > 0f) { LockAxisFor(Axis.Horizontal, axisLockDebugSeconds); axisLockDebugSeconds = 0f; }
    }

    public void SetDirection(Vector2 dir, bool forced = false)
    {
        if (!forced && _lockedAxis != Axis.None && dir != Vector2.zero)
        {
            bool isHoriz = Mathf.Abs(dir.x) >= Mathf.Abs(dir.y);
            if ((_lockedAxis == Axis.Horizontal && !isHoriz) ||
                (_lockedAxis == Axis.Vertical   &&  isHoriz))
            {
                nextDirection = dir;
                return;
            }
        }

        bool accepted = false;
        if (forced || !Occupied(dir))
        {
            direction = dir;
            nextDirection = Vector2.zero;
            accepted = true;
        }
        else
        {
            nextDirection = dir;
        }
        
        if (accepted && _firstMovePending && Mathf.Abs(direction.x) > 0.01f)
        {
            _firstMovePending = false;
            _lockedAxis = Axis.None;
            _axisUnlockAt = 0f;
        }
    }

    // public void SnapToGridCenter()
    // {
    //     var pos = rb.position;
    //     var center = new Vector2(
    //         Mathf.Round(pos.x / tileSize) * tileSize,
    //         Mathf.Round(pos.y / tileSize) * tileSize
    //     );
    //     rb.position = center;
    // }
    //
    // public bool TryResolveStuck(bool trySmallerCast = true)
    // {
    //     if (!Occupied(direction)) return false;
    //     
    //     Vector2 fwd = direction;
    //     Vector2 rev = new Vector2(-direction.x, -direction.y);
    //     bool horiz = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y);
    //     Vector2[] cands = horiz
    //         ? new[] { Vector2.up, Vector2.down, fwd, rev }
    //         : new[] { Vector2.left, Vector2.right, fwd, rev };
    //     
    //     foreach (var d in cands)
    //     {
    //         if (d == rev && !allowReverseWhenStuck) continue;
    //         if (!Occupied(d))
    //         {
    //             direction = d;
    //             nextDirection = Vector2.zero;
    //             return true;
    //         }
    //     }
    //     
    //     if (trySmallerCast)
    //     {
    //         var keepSize = castBoxSize;
    //         var keepDist = castDistance;
    //         castBoxSize *= 0.8f;
    //         castDistance *= 0.8f;
    //
    //         foreach (var d in cands)
    //         {
    //             if (d == rev && !allowReverseWhenStuck) continue;
    //             if (!Occupied(d))
    //             {
    //                 castBoxSize = keepSize;
    //                 castDistance = keepDist;
    //                 direction = d;
    //                 nextDirection = Vector2.zero;
    //                 return true;
    //             }
    //         }
    //
    //         castBoxSize = keepSize;
    //         castDistance = keepDist;
    //     }
    //
    //     return false;
    // }
    
    public void ClearQueuedDirection()
    {
        nextDirection = Vector2.zero;
    }

    public bool Occupied(Vector2 dir)
    {
        if (dir == Vector2.zero) return false;
        
        RaycastHit2D hit = Physics2D.BoxCast(
            rb.position,
            castBoxSize,
            0f,
            dir,
            castDistance,
            obstacleLayer
        );
        return hit.collider != null;
    }
}
