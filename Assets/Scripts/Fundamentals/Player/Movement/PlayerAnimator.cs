using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Pure presentation layer for the player:
/// – Listens to events from <see cref="PlayerMovementController"/> to trigger animations,
///   particle effects, and audio.
/// – Tilts / flips the sprite and scales particles based on movement speed.
/// 
/// No gameplay decisions are made here; everything is cosmetic.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    /* ---------- Inspector References ---------- */
    [Header("References")]
    [SerializeField] private Animator        _anim;
    [FormerlySerializedAs("_player")] [SerializeField] private PlayerMovementController playerMovement;  // Event source
    [SerializeField] private SpriteRenderer  _sprite;
    [SerializeField] private Rigidbody2D     _rb;

    [Header("Settings")]
    [SerializeField, Range(1f, 3f)] private float _maxIdleSpeed = 2; // Idle bob speed multiplier
    [SerializeField] private float _maxTilt  = 5;   // Max sprite tilt in degrees (unused in sample)
    [SerializeField] private float _tiltSpeed = 20; // Speed to tilt towards target rotation

    [Header("Particles")]
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _landParticles;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] _footsteps;

    /* ---------- Internals ---------- */
    private AudioSource _source;
    private bool _grounded;
    private ParticleSystem.MinMaxGradient _currentGradient; // Dynamic color for ground-dependent VFX

    /* ====================================================================== */
    /*                          Unity Lifecycle                               */
    /* ====================================================================== */

    private void Awake() => _source = GetComponent<AudioSource>();

    private void OnEnable()
    {
        // Subscribe to gameplay events
        playerMovement.Jumped          += OnJumped;
        playerMovement.GroundedChanged += OnGroundedChanged;

        _moveParticles.Play(); // Start dust trail immediately
    }

    private void OnDisable()
    {
        playerMovement.Jumped          -= OnJumped;
        playerMovement.GroundedChanged -= OnGroundedChanged;

        _moveParticles.Stop();
    }

    private void Update()
    {
        if (playerMovement == null) return; // Safety for prefab previews

        DetectGroundColor(); // Update particle tint if we change terrain

        HandleSpriteFlip();  // Face movement direction
        HandleIdleSpeed();   // Adjust idle animation speed
        DetectFalling();     // Set Falling bool
        DetectRunning();     // Set Running bool
    }

    /* ====================================================================== */
    /*                      Animation State Helpers                           */
    /* ====================================================================== */

    /// <summary>Sets "Falling" bool based on vertical velocity sign.</summary>
    private void DetectFalling()
    {
        _anim.SetBool(FallingBool, _rb.linearVelocity.y < 0);
    }

    /// <summary>Sets "Running" bool if horizontal speed exceeds threshold.</summary>
    private void DetectRunning()
    {
        bool isIdle = Mathf.Abs(_rb.linearVelocity.x) < 0.1f;
        _anim.SetBool(RunningBool, !isIdle);
    }

    /// <summary>Flips sprite when player walks left/right.</summary>
    private void HandleSpriteFlip()
    {
        if (playerMovement.FrameInput.x != 0)
            _sprite.flipX = playerMovement.FrameInput.x < 0;
    }

    /// <summary>Drive idle bob speed & particle scale from analog stick strength.</summary>
    private void HandleIdleSpeed()
    {
        float inputStrength = Mathf.Abs(playerMovement.FrameInput.x);

        // Idle animation: 1× speed when stick neutral → maxIdleSpeed when fully held
        _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));

        // Dust trail grows / shrinks with speed
        _moveParticles.transform.localScale =
            Vector3.MoveTowards(_moveParticles.transform.localScale,
                                Vector3.one * inputStrength,
                                2 * Time.deltaTime);
    }

    /* ====================================================================== */
    /*                           Event Handlers                               */
    /* ====================================================================== */

    private void OnJumped()
    {
        _anim.SetTrigger(JumpKey);       // Fire jump animation
        _anim.ResetTrigger(GroundedKey); // Prevent landing anim from overriding mid-air

        // Spawn particles only if we actually jumped off ground (ignore coyote)
        if (_grounded)
        {
            SetColor(_jumpParticles);
            SetColor(_launchParticles);

            _jumpParticles.Play();
        }
    }

    private void OnGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;

        if (grounded)
        {
            DetectGroundColor();     // Refresh tint
            SetColor(_landParticles);

            _anim.SetTrigger(GroundedKey);
            _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
            _moveParticles.Play();   // Resume dust trail

            // Scale landing puff based on impact velocity
            _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            _landParticles.Play();
        }
        else
        {
            _moveParticles.Stop();   // Stop dust while airborne
        }
    }

    /* ====================================================================== */
    /*               Dynamic Particle Tint (matches ground)                   */
    /* ====================================================================== */

    /// <summary>Raycasts down to grab the color of the tile we’re standing on.</summary>
    private void DetectGroundColor()
    {
        var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

        if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r))
            return;

        Color groundColor = r.color;
        _currentGradient  = new ParticleSystem.MinMaxGradient(groundColor * 0.9f,
                                                              groundColor * 1.2f);
        SetColor(_moveParticles);
    }

    private void SetColor(ParticleSystem ps)
    {
        var main = ps.main;
        main.startColor = _currentGradient;
    }

    /* ====================================================================== */
    /*                           Animator Hashes                              */
    /* ====================================================================== */

    private static readonly int GroundedKey  = Animator.StringToHash("Grounded");
    private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
    private static readonly int JumpKey      = Animator.StringToHash("Jump");
    private static readonly int FallingBool  = Animator.StringToHash("Falling");
    private static readonly int RunningBool  = Animator.StringToHash("Running");
}
