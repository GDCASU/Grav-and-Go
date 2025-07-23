using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Singleton that owns the <c>PlayerControls</c> asset and surfaces the
/// current input state each frame.
/// </summary>
public class InputManager : MonoBehaviour
{
    // Singleton
    public static InputManager Instance { get; private set;}
    
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog = false; // Gate for spammy logs

    // Input-updated fields
    public Vector2 movementInput { get; private set; } // Left-stick / WASD
    public bool jumpHeldDownInput { get; private set; }  // True while jump button held
    public bool jumpPressedThisFrame { get; private set; }  // True only on the frame pressed
    public bool pullHeldDownInput { get; private set; } // True while pull button held
    public bool didPlayerWheelFoward { get; private set; } // True while the player spins the mousewheel up
    public bool didPlayerWheelBackwards { get; private set; } // True while the player spins the mousewheel down
    public bool didPlayerHoldDownRotate { get; private set; } // True while the player holds down R
    public bool didPlayerClickMouseWheelThisFrame { get; private set; } // True if the player clicked the mouse wheel this frame
    public bool pullPressedThisFrame { get; private set; }  // True only on the frame pressed
    public bool pushPressedThisFrame { get; private set; }  // True only on the frame pressed

    // Local Variables
    private PlayerControls _playerControls;
    private bool _pushInputRecieved; 

    #region Public Events
    
    /// <summary>Raised continuously while Move input is active.</summary>
    public static event Action OnMove;

    /// <summary>Raised on jump press (performed).</summary>
    public static event Action OnJump;
    
    /// <summary> Raised when the player clicks the interact key </summary>
    public static event Action OnInteract;
    
    public static event Action OnPause;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Input System mapping
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
        didPlayerHoldDownRotate = _playerControls.GravityGun.Rotate.IsPressed();
        didPlayerClickMouseWheelThisFrame = _playerControls.GravityGun.Special.WasPerformedThisFrame();
        pullPressedThisFrame = _playerControls.GravityGun.Pull.WasPerformedThisFrame();
        pushPressedThisFrame = _playerControls.GravityGun.Push.WasPerformedThisFrame();
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
    
    #endregion

    #region Binding & Callbacks  

    /// <summary>
    /// Subscribes C# methods to each InputAction. This keeps logic decoupled
    /// from the auto-generated <c>PlayerControls</c> class.
    /// </summary>
    private void BindPlayerEvents()
    {
        // Movement Axis
        _playerControls.Movement.Move.performed += HandleMovementInput;
        _playerControls.Movement.Move.canceled += HandleMovementInput;

        // Gravity Gun Actions 
        _playerControls.GravityGun.Special.performed += HandleSpecial;
        _playerControls.GravityGun.MouseWheelDown.performed += HandleWheelBackwards;
        _playerControls.GravityGun.MouseWheelDown.canceled += HandleWheelBackwards;
        _playerControls.GravityGun.MouseWheelUp.performed += HandleWheelFoward;
        _playerControls.GravityGun.MouseWheelUp.canceled += HandleWheelFoward;
        
        // Interactions
        _playerControls.Interactions.Interact.performed += HandleInteraction;

        // Level / UI Actions
        _playerControls.Level.Retry.performed += HandleLevelRetry;
        _playerControls.UI.Pause.performed += HandlePause;
    }
    
    #endregion
    
    #region Movement

    private void HandleMovementInput(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();

        if (_doDebugLog) Debug.Log($"Movement Input = {movementInput}");
        OnMove?.Invoke();
    }
    
    #endregion
    
    #region Gravity Gun

    // NOTE: Gameplay not implemented yet; some methods are placeholders
    private void HandleSpecial(InputAction.CallbackContext ctx) { /* TODO */ }
    
    private void HandleWheelFoward(InputAction.CallbackContext ctx)
    {
        didPlayerWheelFoward = ctx.performed;
    }

    private void HandleWheelBackwards(InputAction.CallbackContext ctx)
    {
        didPlayerWheelBackwards = ctx.performed;
    }
    
    #endregion

    #region Interactions

    private void HandleInteraction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnInteract?.Invoke();
    }

    #endregion

    #region Level / UI
    
    private void HandleLevelRetry(InputAction.CallbackContext ctx) { /* TODO */ }

    private void HandlePause(InputAction.CallbackContext ctx)
    {
        OnPause?.Invoke();
    }
    
    #endregion

    
}
