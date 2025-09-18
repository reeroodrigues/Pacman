using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedSprite : MonoBehaviour
{
    public Sprite[] sprites = new Sprite[0];
    public float animationTime = 0.25f;
    public bool loop = true;

    private SpriteRenderer _spriteRenderer;
    private int _animationFrame;

    public bool Finished { get; private set; }
    public float TotalDuration => (sprites != null ? sprites.Length : 0) * animationTime;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _spriteRenderer.enabled = true;
        Finished = false;
        
        if (!IsInvoking(nameof(Advance)) && sprites.Length > 0)
            InvokeRepeating(nameof(Advance), animationTime, animationTime);
    }

    private void OnDisable()
    {
        _spriteRenderer.enabled = false;
        CancelInvoke(nameof(Advance));
    }

    private void Start()
    {
        if (!IsInvoking(nameof(Advance)) && sprites.Length > 0)
            InvokeRepeating(nameof(Advance), animationTime, animationTime);
    }

    private void Advance()
    {
        if (!_spriteRenderer.enabled) return;

        _animationFrame++;

        if (_animationFrame >= sprites.Length)
        {
            if (loop)
            {
                _animationFrame = 0;
            }
            else
            {
                Finished = true;
                CancelInvoke(nameof(Advance));
                return;
            }
        }

        if (_animationFrame >= 0 && _animationFrame < sprites.Length)
            _spriteRenderer.sprite = sprites[_animationFrame];
    }

    public void Restart()
    {
        Finished = false;
        _animationFrame = -1;
        
        CancelInvoke(nameof(Advance));
        if (sprites.Length > 0)
            InvokeRepeating(nameof(Advance), 0f, animationTime);
        Advance();
    }
}
