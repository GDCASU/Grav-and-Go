using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using Unity.VisualScripting;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

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
public class PlayerMovementController : MonoBehaviour, IDamageable
{
    // Tuning
    [Header("Tunning")]
    [SerializeField, Tooltip("ScriptableObject containing tunable movement numbers")] private ScriptableStats _stats;

    [Header("Death")]
    [SerializeField, Tooltip("Seconds to wait after death animation before reloading the level.")]
    private float _deathDelaySeconds = 3f;

    // Death state — guard against TakeDamage firing more than once before reload
    private bool _isDead = false;

    [Header("References")]
    [SerializeField] private PlayerAnimator _playerAnimator;
    private Rigidbody2D _rb;
    public CapsuleCollider2D _collider;

    [Header("Audio")]
    [SerializeField] private SimpleAudioEmitter _walkSound;
    [SerializeField] private SimpleAudioEmitter _jumpSound;

    [Header("Capsule Casts")]
    [SerializeField, Tooltip("Offset for the ground check capsule cast, in local space.")]
    private Vector2 _groundCheckOffset = Vector2.zero;

    [SerializeField, Tooltip("Offset for the ceiling check capsule cast, in local space.")]
    private Vector2 _ceilingCheckOffset = Vector2.zero;

    [SerializeField, Tooltip("Small substraction as to make sure capsule casts arent bigger than the collider")]
    private Vector2 _capsuleCastSizeSub = Vector2.zero;

    [Header("Debugging")]
    [SerializeField] private bool _doDrawCapsuleCast;

    [Header("Per-frame state")]
    [SerializeField, Vector2Compass, InspectorReadOnly] public Vector2 movement;    // Movement we gather from input
    [SerializeField, Vector2Compass, InspectorReadOnly] private Vector2 velocity;    // Velocity we write to Rigidbody each FixedUpdate
    [SerializeField, InlineToggle, InspectorReadOnly] private bool _cachedQueryStartInColliders;

    [Header("Collisions")]
    [SerializeField, InspectorReadOnly] private float _frameLeftGrounded = float.MinValue; // Time when feet last left ground
    [SerializeField, InspectorReadOnly] private bool _grounded; // True when capsule is in contact

    [Header("Jump state flags")]
    [SerializeField, InspectorReadOnly] private bool _jumpToConsume;
    [SerializeField, InspectorReadOnly] private bool _bufferedJumpUsable;
    [SerializeField, InspectorReadOnly] private bool _endedJumpEarly;
    [SerializeField, InspectorReadOnly] private bool _coyoteUsable;

    [Header("Timing helpers")]
    [SerializeField, InspectorReadOnly] private float _timeJumpWasPressed;
    [SerializeField, InspectorReadOnly] private float _timeJumpWasReleased;

    [InspectorReadOnly] public TractorBeam _tractorBeam;

    // Private helpers
    private float _time; // Global time accumulator for coyote / buffer windows

    #region Events

    /// <summary>Fired when grounded ↔ airborne; float = landing impact strength.</summary>
    public event Action<bool, float> GroundedChanged;

    /// <summary>Fired the exact frame a jump is executed.</summary>
    public event Action Jumped;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CapsuleCollider2D>(); // not in use?
    }

    private void Awake()
    {
        // Store default‐engine setting so we can temporarily override it
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {
        // Dont do anything if paused
        if (Time.timeScale > 0) _time += Time.deltaTime;

        OutOfBoundsCheck();
    }

    private void FixedUpdate()
    {
        if (_isDead) return;
        
        // Dont do anything if paused
        if (Time.timeScale <= 0) return;

        CheckCollisions();  // Ground / ceiling detection first — movement depends on result

        if(_tractorBeam)
        {
            HandleTractorPull();
        }
        else
        {
			HandleJump();
			HandleGravity();
			ApplyMovement();    // Finally push calculated velocity to the Rigidbody
		}

		_rb.linearVelocity = velocity;
	}

    #endregion

    #region Input Functions
    private void OnMove(InputValue value)
    {
        if (_isDead)
        {
            movement = Vector2.zero;
            return;
        }
        
        movement = value.Get<Vector2>();

        // Optional "digital" snapping so small stick values become full cardinal movement.
        if (_stats.SnapInput)
        {
            movement.x = Mathf.Abs(movement.x) < _stats.HorizontalDeadZoneThreshold
                                 ? 0
                                 : Mathf.Sign(movement.x);

            movement.y = Mathf.Abs(movement.y) < _stats.VerticalDeadZoneThreshold
                                 ? 0
                                 : Mathf.Sign(movement.y);
        }
    }

    private void OnJump(InputValue value)
    {
        if (_isDead) return;
        
        if (value.isPressed)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }
        else
        {
            _timeJumpWasReleased = _time;

            // If player releases jump early, start applying stronger gravity
            if (!_endedJumpEarly && !_grounded && _rb.linearVelocity.y > 0)
            {
                _endedJumpEarly = true;
            }
        }
    }

    #endregion

    #region Collisions

    private void OutOfBoundsCheck()
    {
        if (_isDead) return;

        Camera mainCamera = Camera.main;
        if (!mainCamera) return;

        // orthographicSize is half-height only; width requires aspect ratio
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth  = halfHeight * mainCamera.aspect;

        Vector3 camPos = mainCamera.transform.position;
        Rect cameraRect = new Rect(
            camPos.x - halfWidth,
            camPos.y - halfHeight,
            halfWidth  * 2f,
            halfHeight * 2f
        );

        // Kill the player only when fully outside — both bounds corners are out
        if (!cameraRect.Contains(_collider.bounds.min) && !cameraRect.Contains(_collider.bounds.max))
        {
            Debug.Log("[PlayerMovementController] Player is out of bounds!");
            TakeDamage(int.MaxValue, null);
        }
    }

    /// <summary>CapsuleCasts for ground & ceiling each physics step.</summary>
    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false; // More reliable casts

        // Ground cast with offset
        Vector2 groundStart = (Vector2)_collider.bounds.center + _groundCheckOffset;
        bool groundHit = Physics2D.CapsuleCast(groundStart, _collider.size - _capsuleCastSizeSub, _collider.direction, 0,
            Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);

        // Ceiling cast with offset
        Vector2 ceilingStart = (Vector2)_collider.bounds.center + _ceilingCheckOffset;
        bool ceilingHit = Physics2D.CapsuleCast(ceilingStart, _collider.size - _capsuleCastSizeSub, _collider.direction, 0,
            Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

        // If we bonk our head, kill any upward momentum
        if (ceilingHit)
            velocity.y = Mathf.Min(0, velocity.y);

        // ------------ Ground-state transitions ------------
        if (!_grounded && groundHit) // Landed
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(velocity.y));
        }
        else if (_grounded && !groundHit)      // Left ground
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders; // Restore default
    }

    #endregion

    private void HandleTractorPull()
    {
        Vector2 targetVelocity = _tractorBeam.PullSpeed * _tractorBeam.GetPullVector();
        Vector2 difference = targetVelocity - velocity;
        if(difference.magnitude > _tractorBeam.ObjectDeceleration * Time.fixedDeltaTime)
        {
            velocity += _tractorBeam.ObjectDeceleration * Time.fixedDeltaTime * difference.normalized;
        }
        else
        {
            velocity = targetVelocity;
        }

        // Prevent auto jump upon leaving tractor beam
		_jumpToConsume = false;
	}

	#region Jumping

	// Convenience properties
	private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded &&
                                    _time < _frameLeftGrounded + _stats.CoyoteTime;

    /// <summary>Processes buffered / coyote jumps and variable jump height.</summary>
    private void HandleJump()
    {
        // Nothing to do if we didn't press jump or buffer expired
        if (!_jumpToConsume && !HasBufferedJump) return;

        // Execute if grounded or still within coyote window
        if (_grounded || CanUseCoyote) ExecuteJump();

        _jumpToConsume = false; // Reset buffer regardless
    }

    /// <summary>Performs the actual jump impulse.</summary>
    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _jumpSound.PlaySound();

        velocity.y = _stats.JumpPower; // Instant upward velocity
        Jumped?.Invoke();
    }

    public void AddVelocity(Vector2 force)
    {
        velocity += force;
    }

    #endregion

    #region Gravity

    /// <summary>Custom gravity that supports fast-fall & jump-cut.</summary>
    private void HandleGravity()
    {
        if (_grounded && velocity.y <= 0f)
        {
            // Stick player to ground (prevents tiny bounces on slopes)
            velocity.y = _stats.GroundingForce;
        }
        else
        {
            float gravity = _stats.FallAcceleration;

            // If jump was cut early, increase downward pull
            if (_endedJumpEarly && velocity.y > 0)
                gravity *= _stats.JumpEndEarlyGravityModifier;

            // Gradually move toward terminal fall speed
            velocity.y = Mathf.MoveTowards(velocity.y,
                                                 -_stats.MaxFallSpeed,
                                                 gravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region RigidBody Application

    /// <summary>Writes the fully-calculated velocity to the Rigidbody2D.</summary>
    private void ApplyMovement()
    {
        if (Mathf.Approximately(movement.x, 0f)) // No horizontal input → decelerate towards 0
        {
            var decel = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, 0, decel * Time.fixedDeltaTime);
        }
        else                                    // Accelerate towards target speed in chosen direction
        {
            velocity.x = Mathf.MoveTowards(
                velocity.x,
                movement.x * _stats.MaxSpeed,
                _stats.Acceleration * Time.fixedDeltaTime
            );

            // Only play walk sound if grounded
            if (_grounded) _walkSound.PlaySound();
        }
    }

    #endregion

    public void TakeDamage(int damage, Rigidbody2D rb)
    {
        Die();
    }

    /// <summary>
    /// Triggers the player death sequence:
    /// freezes movement, plays the death animation,
    /// then reloads the current level after <see cref="_deathDelaySeconds"/>.
    /// Safe to call multiple times — does nothing if already dead.
    /// </summary>
    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // Freeze all movement
        velocity = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;

        // Trigger death animation
        if (_playerAnimator != null)
            _playerAnimator.OnDeath();
        else
            Debug.LogWarning("[PlayerMovementController] _playerAnimator is not assigned — skipping death animation.");

        StartCoroutine(DeathReloadRoutine());
    }

    /// <summary>
    /// Waits for the death animation to finish, then reloads the current level.
    /// </summary>
    private IEnumerator DeathReloadRoutine()
    {
        yield return new WaitForSeconds(_deathDelaySeconds);
        LevelManager.Instance.ReloadCurrentLevel();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_doDrawCapsuleCast) return;

        if (_collider == null || _stats == null) return;

        var center = (Vector2)_collider.bounds.center;
        var size = _collider.size - _capsuleCastSizeSub;
        var dir = _collider.direction;
        float dist = _stats.GrounderDistance;

        // Ground cast (down)
        DrawCapsuleCastGizmo(center + _groundCheckOffset, size, dir, Vector2.down, dist,
            _grounded ? new Color(0f, 1f, 0f, 1f) : new Color(1f, 0f, 0f, 1f));

        // Ceiling cast (up)
        DrawCapsuleCastGizmo(center + _ceilingCheckOffset, size, dir, Vector2.up, dist,
            new Color(0f, 1f, 1f, 1f));
    }

    /// <summary>Draws start capsule, end capsule, and an arrow for a 2D CapsuleCast.</summary>
    private void DrawCapsuleCastGizmo(Vector2 center, Vector2 size, CapsuleDirection2D capsuleDir,
        Vector2 direction, float distance, Color color)
    {
        // Start capsule
        Handles.color = color;
        DrawWireCapsule2D(center, size, capsuleDir);

        // End capsule
        var endCenter = center + direction.normalized * distance;
        Handles.color = new Color(color.r, color.g, color.b, 0.35f);
        DrawWireCapsule2D(endCenter, size, capsuleDir);

        // Arrow between start and end
        Handles.color = color;
        Handles.DrawLine(center, endCenter);

        // Arrow head
        var headLen = Mathf.Min(size.x, size.y) * 0.25f;
        var dirNorm = direction.normalized;
        var left = Quaternion.Euler(0, 0, 135) * dirNorm * headLen;
        var right = Quaternion.Euler(0, 0, -135) * dirNorm * headLen;
        Handles.DrawLine(endCenter, endCenter + (Vector2)left);
        Handles.DrawLine(endCenter, endCenter + (Vector2)right);
    }

    /// <summary>Editor only helper to draw a 2D capsule outline using two discs and two lines.</summary>
    private static void DrawWireCapsule2D(Vector2 center, Vector2 size, CapsuleDirection2D capsuleDir)
    {
        if (capsuleDir == CapsuleDirection2D.Vertical)
        {
            float radius = size.x * 0.5f;
            float straight = Mathf.Max(0f, size.y - 2f * radius);

            var topCenter = center + Vector2.up * (straight * 0.5f);
            var bottomCenter = center - Vector2.up * (straight * 0.5f);

            Handles.DrawWireDisc(topCenter, Vector3.forward, radius);
            Handles.DrawWireDisc(bottomCenter, Vector3.forward, radius);

            var leftOffset = Vector2.left * radius;
            var rightOffset = Vector2.right * radius;

            Handles.DrawLine(topCenter + leftOffset, bottomCenter + leftOffset);
            Handles.DrawLine(topCenter + rightOffset, bottomCenter + rightOffset);
        }
        else // Horizontal
        {
            float radius = size.y * 0.5f;
            float straight = Mathf.Max(0f, size.x - 2f * radius);

            var rightCenter = center + Vector2.right * (straight * 0.5f);
            var leftCenter = center - Vector2.right * (straight * 0.5f);

            Handles.DrawWireDisc(rightCenter, Vector3.forward, radius);
            Handles.DrawWireDisc(leftCenter, Vector3.forward, radius);

            var upOffset = Vector2.up * radius;
            var downOffset = Vector2.down * radius;

            Handles.DrawLine(rightCenter + upOffset, leftCenter + upOffset);
            Handles.DrawLine(rightCenter + downOffset, leftCenter + downOffset);
        }
    }

#endif
}