using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// Singleton that compiles all user input and surfaces the
/// current input state each frame.
/// </summary>
public class InputManager : MonoBehaviour
{
    /* ---------- Singleton Boilerplate ---------- */
    public static InputManager Instance;

    /* ---------- Inspector Flags ---------- */
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog = false; // Gate for spammy logs

    /* ---------- Input-updated fields ---------- */
    public Vector2 movementInput { get; private set; } // Left-stick / WASD
    public bool jumpHeldDownInput { get; private set; }  // True while jump button held
    public bool jumpPressedThisFrame { get; private set; }  // True only on the frame pressed
    public bool pullHeldDownInput { get; private set; }
    private bool _pushInputRecieved;
    public bool didPlayerRotateFoward { get; private set; }
    public bool didPlayerRotateBackwards { get; private set; }

    /* ---------- Private vars ---------- */
    private PlayerControls _playerControls;

    #region Public Events
    
    /// <summary>Raised continuously while Move input is active.</summary>
    public static event Action OnMove;

    /// <summary>Raised on jump press (performed).</summary>
    public static event Action OnJump;
    
    /// <summary> Raised when the player clicks the interact key </summary>
    public static event Action OnInteract;
    
    public static event Action OnPause;

    #endregion

    /* ====================================================================== */
    /*                          Unity Lifecycle                               */
    /* ====================================================================== */

    private void Awake()
    {
        /* -------- Singleton enforcement -------- */
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        /* -------- Input System mapping -------- */
        if (_playerControls == null)
        {
            _playerControls = new PlayerControls();
            BindPlayerEvents();
        }

        _playerControls.Enable(); // Enable after binding
    }

    private void Update()
    {
        // Poll button states every frame – cheapest way to expose booleans
        jumpPressedThisFrame = _playerControls.Movement.Jump.WasPerformedThisFrame();
        jumpHeldDownInput = _playerControls.Movement.Jump.IsPressed();
        pullHeldDownInput = _playerControls.GravityGun.Pull.IsPressed();
        if (_playerControls.GravityGun.Push.WasPerformedThisFrame())
        {
            // This one requires special attention due to the gravity gun using fixedUpdate
            _pushInputRecieved = true;
        }
        
    }

    private void OnDestroy()
    {
        // Only disable controls if *we* are the active singleton
        if (Instance == this)
        {
            _playerControls?.Disable();
            Instance = null;
        }
        StopAllCoroutines();
    }

    /* ====================================================================== */
    /*                         Binding & Callbacks                            */
    /* ====================================================================== */

    /// <summary>
    /// Subscribes C# methods to each InputAction. This keeps logic decoupled
    /// from the auto-generated <c>PlayerControls</c> class.
    /// </summary>
    private void BindPlayerEvents()
    {
        /* -------- Movement Axis -------- */
        _playerControls.Movement.Move.performed += HandleMovementInput;
        _playerControls.Movement.Move.canceled += HandleMovementInput;

        /* -------- Gravity Gun Actions -------- */
        _playerControls.GravityGun.Push.performed += HandlePush;
        _playerControls.GravityGun.Pull.performed += HandlePull;
        _playerControls.GravityGun.Special.performed += HandleSpecial;
        _playerControls.GravityGun.RotateObjectBackwards.performed += HandleRotateBackwards;
        _playerControls.GravityGun.RotateObjectBackwards.canceled += HandleRotateBackwards;
        _playerControls.GravityGun.RotateObjectFoward.performed += HandleRotateFoward;
        _playerControls.GravityGun.RotateObjectFoward.canceled += HandleRotateFoward;
        
        /* -------- Interactions -------- */
        _playerControls.Interactions.Interact.performed += HandleInteraction;

        /* -------- Level / UI Actions -------- */
        _playerControls.Level.Retry.performed += HandleLevelRetry;
        _playerControls.UI.Pause.performed += HandlePause;
    }

    /* ---------------- Movement ---------------- */

    private void HandleMovementInput(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();

        if (_doDebugLog) Debug.Log($"Movement Input = {movementInput}");
        OnMove?.Invoke();
    }

    /* --------------- Gravity Gun -------------- */
    // NOTE: Gameplay not implemented yet; methods are placeholders
    private void HandlePush   (InputAction.CallbackContext ctx) { /* TODO */ }
    private void HandlePull  (InputAction.CallbackContext ctx) { /* TODO */ }
    private void HandleSpecial(InputAction.CallbackContext ctx) { /* TODO */ }

    private void HandleRotateFoward(InputAction.CallbackContext ctx)
    {
        didPlayerRotateFoward = ctx.performed;
    }

    private void HandleRotateBackwards(InputAction.CallbackContext ctx)
    {
        didPlayerRotateBackwards = ctx.performed;
    }
    
    /* -------- Interactions -------- */

    private void HandleInteraction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnInteract?.Invoke();
    }

    /* ---------------- Level / UI -------------- */
    private void HandleLevelRetry(InputAction.CallbackContext ctx) { /* TODO */ }

    private void HandlePause(InputAction.CallbackContext ctx)
    {
        OnPause?.Invoke();
    }
    
    // Getters
    public bool PopPushInputRecieved()
    {
        bool value = _pushInputRecieved;
        _pushInputRecieved = false;
        return value;
    }
}
