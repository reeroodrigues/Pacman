using UnityEngine;

[DefaultExecutionOrder(10)]
[RequireComponent(typeof(Ghost))]
public class BlinkyStartOnPacmanMove : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Pacman pacman;          
    [SerializeField] private bool reenableScatter = true;

    [Header("Config")]
    [SerializeField] private float waitTime = 3f;
    [SerializeField] private Vector2 fallbackDirection = Vector2.left;

    private Ghost _ghost;
    private Movement _ghostMove;
    private Movement _pacMove;

    private bool _started;
    private float _timer;

    private Vector2 _pacStartPos;

    private void Awake()
    {
        _ghost = GetComponent<Ghost>();
        _ghostMove = GetComponent<Movement>();
    }

    private void Start()
    {
        if (!pacman) pacman = FindObjectOfType<Pacman>();
        if (pacman) _pacMove = pacman.GetComponent<Movement>();

        if (_pacMove != null)
            _pacStartPos = (Vector2)_pacMove.startingPosition;

        ArmHold();
    }

    private void Update()
    {
        if (_pacMove == null) return;

        if (!_started)
        {
            _timer += Time.deltaTime;
            HoldGhost();
            
            if (_pacMove.direction != Vector2.zero)
            {
                StartBlinkyWithPacmanDirection(_pacMove.direction);
                _started = true;
            }
            else if (_timer >= waitTime)
            {
                StartBlinkyWithPacmanDirection(fallbackDirection);
                _started = true;
            }
        }
    }

    private void ArmHold()
    {
        _started = false;
        _timer = 0f;
        HoldGhost();
    }

    private void HoldGhost()
    {
        if (_ghostMove == null) return;

        if (_ghost.Home)       _ghost.Home.enabled = false;
        if (_ghost.Scatter)    _ghost.Scatter.enabled = false;
        if (_ghost.Chase)      _ghost.Chase.enabled = false;
        if (_ghost.Frightened) _ghost.Frightened.enabled = false;

        _ghostMove.rb.isKinematic = true;
        _ghostMove.enabled = false;
        _ghostMove.ClearQueuedDirection();
    }

    private void StartBlinkyWithPacmanDirection(Vector2 dir)
    {
        if (_ghostMove == null) return;

        _ghostMove.enabled = true;
        _ghostMove.rb.isKinematic = false;

        Vector2 startDir = Vector2.zero;
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            startDir = new Vector2(Mathf.Sign(dir.x), 0f);
        else
            startDir = new Vector2(0f, Mathf.Sign(dir.y));

        _ghostMove.SetDirection(startDir, true);
        
        if (_ghost.Scatter) _ghost.Scatter.Disable();
        if (_ghost.Chase) _ghost.Chase.Enable();
    }
}
