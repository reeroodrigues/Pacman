using System;
using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Movement))]
public class Ghost : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private SoundEvent sfxGhostEaten;
    [SerializeField] private SoundEvent sfxReturnToHome;
    // [SerializeField] private bool resolveOnFrightenedExit = true;

    public Movement Movement { get; private set; }
    public GhostHome Home { get; private set; }
    public GhostScatter Scatter { get; private set; }
    public GhostChase Chase { get; private set; }
    public GhostFrightened Frightened { get; private set; }
    public GhostBehavior initialBehavior;
    public Transform target;
    public int points = 200;
    // private bool _wasFrightened;
    // private int _wallContactFrames = 0;
    // private const int WallContactFrameThreshold = 6;

    private void Awake()
    {
        Movement   = GetComponent<Movement>();
        Home       = GetComponent<GhostHome>();
        Scatter    = GetComponent<GhostScatter>();
        Chase      = GetComponent<GhostChase>();
        Frightened = GetComponent<GhostFrightened>();
    }

    private void Start()
    {
        ResetState();
    }

    // private void Update()
    // {
    //     bool nowFrightened = Frightened != null && Frightened.enabled;
    //     
    //     if (resolveOnFrightenedExit && _wasFrightened && !nowFrightened)
    //     {
    //         if (Movement != null && Movement.Occupied(Movement.direction))
    //         {
    //             Movement.SnapToGridCenter();
    //             Movement.TryResolveStuck(trySmallerCast: true);
    //         }
    //     }
    //
    //     _wasFrightened = nowFrightened;
    // }

    public void ResetState()
    {
        gameObject.SetActive(true);
        Movement.ResetState();

        Frightened.Disable();
        Chase.Disable();
        Scatter.Enable();

        if (Home != initialBehavior) Home.Disable();
        if (initialBehavior != null) initialBehavior.Enable();
        // _wasFrightened = Frightened != null  && Frightened.enabled;
    }

    public void SetPosition(Vector3 position)
    {
        position.z = transform.position.z;
        transform.position = position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            if (Frightened.enabled)
            {
                if (AudioManager.I != null)
                {
                    if(sfxGhostEaten) AudioManager.I.PlayAt(sfxGhostEaten, transform.position);
                    if(sfxReturnToHome) AudioManager.I.PlayAt(sfxReturnToHome, transform.position);
                }
                GameManager.Instance.GhostEaten(this);
            }
            else
            {
                GameManager.Instance.PacmanEaten();
            }
        }
    }
}
