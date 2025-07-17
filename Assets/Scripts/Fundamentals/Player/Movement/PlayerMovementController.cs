using System;
using UnityEngine;

/// <summary>
/// Handles all core movement logic for the player:
/// <para> – Reads per-frame input (via <see cref="InputManager"/>) </para>
/// <para> – Applies horizontal acceleration / deceleration </para>
/// <para> – Implements variable-height jumping with coyote-time and jump-buffer </para>
/// <para> – Calculates custom gravity and grounding logic </para>
///
/// <para> This script *does not* animate or play VFX/SFX directly; those concerns live
/// in <see cref="PlayerAnimator"/>. </para>
/// </summary>
public class PlayerMovementController : MonoBehaviour
{
    // Tuning
    [Header("Tunning")]
    [SerializeField, Tooltip("ScriptableObject containing tunable movement numbers")] private ScriptableStats _stats;

    [Header("References")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private CapsuleCollider2D _col;
    
    [Header("Per-frame state")]
    [SerializeField, Vector2Compass, InspectorReadOnly] private Vector2 _frameVelocity;    // Velocity we write to Rigidbody each FixedUpdate
    [SerializeField, InlineToggle, InspectorReadOnly] private bool _cachedQueryStartInColliders;
    
    
    [Header("Collisions")]
    [SerializeField, InspectorReadOnly] private float _frameLeftGrounded = float.MinValue; // Time when feet last left ground
    [SerializeField, InspectorReadOnly] private bool  _grounded; // True when capsule is in contact
    
    [Header("Jump state flags")]
    [SerializeField, InspectorReadOnly] private bool _jumpToConsume;
    [SerializeField, InspectorReadOnly] private bool _bufferedJumpUsable;
    [SerializeField, InspectorReadOnly] private bool _endedJumpEarly;
    [SerializeField, InspectorReadOnly] private bool _coyoteUsable;
    
    [Header("Timing helpers")]
    [SerializeField, InspectorReadOnly] private float _timeJumpWasPressed;
    
    // Public Helpers
    /// <summary>Expose last-read movement input so external scripts (e.g. PlayerAnimator) can inspect facing direction.</summary>
    public Vector2 frameInputMoveVector => _frameInput.Move;
    
    // Private helpers
    private FrameInput _frameInput; // Raw input sampled this fra
    private float _time; // Global time accumulator for coyote / buffer windows

    #region Events

    /// <summary>Fired when grounded ↔ airborne; float = landing impact strength.</summary>
    public event Action<bool, float> GroundedChanged;

    /// <summary>Fired the exact frame a jump is executed.</summary>
    public event Action Jumped;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        // Store default‐engine setting so we can temporarily override it
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {
        // Dont do anything if paused
        if (Time.timeScale <= 0) return;
        
        _time += Time.deltaTime;
        GatherInput();        // Poll Input System each render-frame
    }
    
    private void FixedUpdate()
    {
        // Dont do anything if paused
        if (Time.timeScale <= 0) return;
        
        CheckCollisions();  // Ground / ceiling detection first — movement depends on result
        
        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();    // Finally push calculated velocity to the Rigidbody
    }

    #endregion
    
    #region Input Gathering

    /// <summary>Polls InputManager and normalizes the data into <see cref="_frameInput"/>.</summary>
    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            JumpDown = InputManager.Instance.jumpPressedThisFrame,
            JumpHeld = InputManager.Instance.jumpHeldDownInput,
            Move     = InputManager.Instance.movementInput
        };

        // Optional "digital" snapping so small stick values become full cardinal movement.
        if (_stats.SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold
                                 ? 0
                                 : Mathf.Sign(_frameInput.Move.x);

            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold
                                 ? 0
                                 : Mathf.Sign(_frameInput.Move.y);
        }

        // Buffer jump so it can be consumed in FixedUpdate (safer than reading in physics step)
        if (_frameInput.JumpDown)
        {
            _jumpToConsume      = true;
            _timeJumpWasPressed = _time;
        }
    }

    #endregion

    #region Collisions
    
    /// <summary>CapsuleCasts for ground & ceiling each physics step.</summary>
    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false; // More reliable casts

        // ------------ Ground & ceiling checks ------------
        bool groundHit  = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0,
            Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0,
            Vector2.up,   _stats.GrounderDistance, ~_stats.PlayerLayer);

        // If we bonk our head, kill any upward momentum
        if (ceilingHit)
            _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // ------------ Ground-state transitions ------------
        if (!_grounded && groundHit)           // Landed
        {
            _grounded            = true;
            _coyoteUsable        = true;
            _bufferedJumpUsable  = true;
            _endedJumpEarly      = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        else if (_grounded && !groundHit)      // Left ground
        {
            _grounded           = false;
            _frameLeftGrounded  = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders; // Restore default
    }

    #endregion

    #region Jumping

    // Convenience properties
    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote    => _coyoteUsable      && !_grounded &&
                                    _time < _frameLeftGrounded + _stats.CoyoteTime;

    /// <summary>Processes buffered / coyote jumps and variable jump height.</summary>
    private void HandleJump()
    {
        // If player releases jump early, start applying stronger gravity
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0)
            _endedJumpEarly = true;

        // Nothing to do if we didn't press jump or buffer expired
        if (!_jumpToConsume && !HasBufferedJump) return;

        // Execute if grounded or still within coyote window
        if (_grounded || CanUseCoyote) ExecuteJump();

        _jumpToConsume = false; // Reset buffer regardless
    }

    /// <summary>Performs the actual jump impulse.</summary>
    private void ExecuteJump()
    {
        _endedJumpEarly      = false;
        _timeJumpWasPressed  = 0;
        _bufferedJumpUsable  = false;
        _coyoteUsable        = false;

        _frameVelocity.y = _stats.JumpPower; // Instant upward velocity
        Jumped?.Invoke();
    }
    
    #endregion
    
    #region Horizontal Acceleration 

    /// <summary>Applies ground/air accel & decel to _frameVelocity.x each physics step.</summary>
    private void HandleDirection()
    {
        if (_frameInput.Move.x == 0) // No horizontal input → decelerate towards 0
        {
            var decel = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, decel * Time.fixedDeltaTime);
        }
        else                         // Accelerate towards target speed in chosen direction
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x,
                                                 _frameInput.Move.x * _stats.MaxSpeed,
                                                 _stats.Acceleration * Time.fixedDeltaTime);
        }
    }
    
    #endregion

    #region Gravity

    /// <summary>Custom gravity that supports fast-fall & jump-cut.</summary>
    private void HandleGravity()
    {
        if (_grounded && _frameVelocity.y <= 0f)
        {
            // Stick player to ground (prevents tiny bounces on slopes)
            _frameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            float gravity = _stats.FallAcceleration;

            // If jump was cut early, increase downward pull
            if (_endedJumpEarly && _frameVelocity.y > 0)
                gravity *= _stats.JumpEndEarlyGravityModifier;

            // Gradually move toward terminal fall speed
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y,
                                                 -_stats.MaxFallSpeed,
                                                 gravity * Time.fixedDeltaTime);
        }
    }
    
    #endregion
    
    #region RigidBody Application

    /// <summary>Writes the fully-calculated velocity to the Rigidbody2D.</summary>
    private void ApplyMovement()
    {
        _rb.linearVelocity = _frameVelocity;
    }
    
    #endregion
    
    /// <summary>Lightweight struct for passing a single frame’s worth of input around.</summary>
    private struct FrameInput
    {
        public bool   JumpDown;
        public bool   JumpHeld;
        public Vector2 Move;
    }
}


