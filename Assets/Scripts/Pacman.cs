using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(Movement))]
public class Pacman : MonoBehaviour
{
    [Header("Visual/FX")]
    [SerializeField] private AnimatedSprite deathSequence;
    [SerializeField] private float deathExtraDelay = 0.1f;
    [SerializeField] private SoundEvent sfxDeath;
    
    [Header("Callbacks")]
    [SerializeField] private UnityEvent onDeathAnimationFinished;
    
    [Header("Input")]
    [SerializeField] private VirtualJoystick joystick;
    [SerializeField] private bool enableKeyboard = true;
    [SerializeField] private float stickThreshold = 0.45f;
    [SerializeField] private float repeatCooldown = 0.12f;

    private SpriteRenderer _spriteRenderer;
    private CircleCollider2D _circleCollider;
    private Movement _movement;

    private Vector2 _lastSentDir = Vector2.zero;
    private float _lastStickSendTime;
    private Vector3 _startingPosition;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _circleCollider = GetComponent<CircleCollider2D>();
        _movement = GetComponent<Movement>();
        _startingPosition = transform.position;
    }

    private void Update()
    {
        if (enableKeyboard)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) _movement.SetDirection(Vector2.up);
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) _movement.SetDirection(Vector2.down);
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) _movement.SetDirection(Vector2.left);
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) _movement.SetDirection(Vector2.right);
        }
        
        if (joystick != null)
        {
            Vector2 jdir = joystick.Direction;
            if (jdir.sqrMagnitude > 0.001f)
            {
                Vector2 card = Mathf.Abs(jdir.x) > Mathf.Abs(jdir.y)
                    ? (jdir.x > 0 ? Vector2.right : Vector2.left)
                    : (jdir.y > 0 ? Vector2.up : Vector2.down);

                if (card != _lastSentDir)
                {
                    _movement.SetDirection(card);
                    _lastSentDir = card;
                }
            }
        }
        
        var pad = Gamepad.current;
        if (pad != null)
        {
            if (pad.dpad.up.wasPressedThisFrame)    { _movement.SetDirection(Vector2.up);    _lastSentDir = Vector2.up; }
            else if (pad.dpad.down.wasPressedThisFrame){ _movement.SetDirection(Vector2.down);  _lastSentDir = Vector2.down; }
            else if (pad.dpad.left.wasPressedThisFrame){ _movement.SetDirection(Vector2.left);  _lastSentDir = Vector2.left; }
            else if (pad.dpad.right.wasPressedThisFrame){ _movement.SetDirection(Vector2.right); _lastSentDir = Vector2.right; }
            else
            {
                var stick = pad.leftStick.ReadValue();
                if (stick.magnitude >= stickThreshold)
                {
                    var card = Mathf.Abs(stick.x) > Mathf.Abs(stick.y)
                        ? (stick.x > 0 ? Vector2.right : Vector2.left)
                        : (stick.y > 0 ? Vector2.up : Vector2.down);

                    if (card != _lastSentDir || (Time.unscaledTime - _lastStickSendTime) >= repeatCooldown)
                    {
                        _movement.SetDirection(card);
                        _lastSentDir = card;
                        _lastStickSendTime = Time.unscaledTime;
                    }
                }
            }
        }
        
        var angle = Mathf.Atan2(_movement.direction.y, _movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    public void ResetState()
    {
        StopAllCoroutines();
        transform.position = _startingPosition;

        enabled = true;
        _movement.enabled = true;
        _circleCollider.enabled = true;
        _spriteRenderer.enabled = true;

        if (deathSequence != null)
        {
            deathSequence.gameObject.SetActive(false);
            deathSequence.enabled = false;
        }

        gameObject.SetActive(true);
        _lastSentDir = Vector2.zero;
        _lastStickSendTime = 0f;
        
        _movement.ResetState();
    }

    public void DeathSequence()
    {
        enabled = false;
        _movement.enabled = false;
        _circleCollider.enabled = false;
        _spriteRenderer.enabled = false;
        
        if (sfxDeath) AudioManager.I.Play2D(sfxDeath);

        if (deathSequence != null)
        {
            deathSequence.transform.position = transform.position;
            deathSequence.gameObject.SetActive(true);
            deathSequence.loop = false;
            deathSequence.enabled = true;
            deathSequence.Restart();

            StartCoroutine(WaitDeathThenGameOver());
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPacmanDeathAnimationFinished();
        }
    }

    private IEnumerator WaitDeathThenGameOver()
    {
        var wait = (deathSequence != null ? deathSequence.TotalDuration : 0f) + deathExtraDelay;
        yield return new WaitForSeconds(wait);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPacmanDeathAnimationFinished();
        }
    }
}
