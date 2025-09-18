using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PacmanMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private VirtualDpad joystick;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask wallLayer;
    

    [Header("Grid")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float nudgeAfterHit = 0.04f;
    
    private bool IsWall(Collider2D col) => ((wallLayer.value & (1 << col.gameObject.layer)) != 0);

    private Rigidbody2D _rb;
    private Vector2 _currentDir = Vector2.right;
    private Vector2 _desiredDir = Vector2.right;
    
    private const float CHECK_DIST = 0.08f;
    private const float SKIN = 0.04f;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[1];
    private ContactFilter2D _filter;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        _filter = new ContactFilter2D { useLayerMask = true, layerMask = wallLayer, useTriggers = false };
    }

    private void Start()
    {
        _rb.linearVelocity = _currentDir * moveSpeed;
    }

    private void Update()
    {
        Vector2 inVec = joystick ? (joystick.Direction != Vector2.zero ? joystick.Direction : joystick.direction) : Vector2.zero;

        if (inVec.magnitude > 0.5f)
        {
            _desiredDir = Mathf.Abs(inVec.x) > Mathf.Abs(inVec.y)
                ? Vector2.right * Mathf.Sign(inVec.x)
                : Vector2.up    * Mathf.Sign(inVec.y);
        }
    }

    private void FixedUpdate()
    {
        TryApplyDesiredDirection();

        if (CanMove(_currentDir))
        {
            _rb.linearVelocity = _currentDir * moveSpeed;
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
            
            HardCenterOnCorridor();
            NudgeFromWall();
            TryResumeAfterHit();
        }
    }
    
    private void HardCenterOnCorridor()
    {
        if (_currentDir == Vector2.zero) return;

        var p = transform.position;

        if (Mathf.Abs(_currentDir.x) > 0f)
            p.y = Mathf.Round(p.y / cellSize) * cellSize;
        else 
            p.x = Mathf.Round(p.x / cellSize) * cellSize;

        transform.position = p;
    }
    
    private void NudgeFromWall()
    {
        if (_currentDir == Vector2.zero) return;
        transform.position += (Vector3)(_currentDir.normalized * nudgeAfterHit);
    }
    
    private void TryResumeAfterHit()
    {
        if (CanMove(_currentDir))
            _rb.linearVelocity = _currentDir * moveSpeed;
        else
            _rb.linearVelocity = Vector2.zero;
    }

    private void TryApplyDesiredDirection()
    {
        if (_desiredDir == _currentDir) return;
        if (!CanMove(_desiredDir)) return;
        
        Vector3 p = transform.position;
        if (Mathf.Abs(_desiredDir.x) > 0f)
            p.y = Mathf.Round(p.y / cellSize) * cellSize;
        else
            p.x = Mathf.Round(p.x / cellSize) * cellSize;

        transform.position = p;
        _currentDir = _desiredDir;
    }

    private void AutoCenterOnCorridor(bool soft)
    {
        if (_currentDir == Vector2.zero) return;

        Vector3 p = transform.position;

        if (Mathf.Abs(_currentDir.x) > 0f)
        {
            float targetY = Mathf.Round(p.y / cellSize) * cellSize;
            p.y = soft ? Mathf.Lerp(p.y, targetY, Time.fixedDeltaTime * 24f)
                : targetY;
        }
        else
        {
            float targetX = Mathf.Round(p.x / cellSize) * cellSize;
            p.x = soft ? Mathf.Lerp(p.x, targetX, Time.fixedDeltaTime * 24f)
                : targetX;
        }

        transform.position = p;
    }

    private bool CanMove(Vector2 dir)
    {
        if (dir == Vector2.zero) return false;
        
        float dist = moveSpeed * Time.fixedDeltaTime + SKIN;
        int count = _rb.Cast(dir, _filter, _hits, dist);
        return count == 0;
    }
    
    public void SetDesiredDirection(Vector2 dir)  => _desiredDir  = dir;
    public void ForceCurrentDirection(Vector2 dir) => _currentDir = dir;
}