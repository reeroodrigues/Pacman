using UnityEngine;

public class GamepadInput : MonoBehaviour
{
    [SerializeField] private PacmanMovement pacman;
    [SerializeField, Range(0.2f, 0.9f)] private float threshold = 0.5f;

    private PacmanInput _input;
    private Vector2 _raw;

    private void Awake()
    {
        if (!pacman) pacman = GetComponent<PacmanMovement>();
        _input = new PacmanInput();
    }

    private void OnEnable()
    {
        _input.Enable();
        _input.Gameplay.Move.performed += ctx => _raw = ctx.ReadValue<Vector2>();
        _input.Gameplay.Move.canceled  += ctx => _raw = Vector2.zero;
    }

    private void OnDisable()
    {
        _input.Gameplay.Move.performed -= ctx => _raw = ctx.ReadValue<Vector2>();
        _input.Gameplay.Move.canceled  -= ctx => _raw = Vector2.zero;
        _input.Disable();
    }

    private void Update()
    {
        if (_raw.magnitude < threshold) return;

        Vector2 desired = Mathf.Abs(_raw.x) > Mathf.Abs(_raw.y)
            ? Vector2.right * Mathf.Sign(_raw.x)
            : Vector2.up    * Mathf.Sign(_raw.y);

        pacman.SetDesiredDirection(desired);
    }
}