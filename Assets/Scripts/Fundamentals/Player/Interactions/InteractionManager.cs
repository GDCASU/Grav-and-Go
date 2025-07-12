using UnityEngine;

/// <summary>
/// 2-D interaction manager that looks for the nearest Interactable inside
/// an OverlapCircle each frame (with Scene-view gizmos for tuning).
/// </summary>
public class InteractionManager2D : MonoBehaviour
{
    /* ────────── Inspector ────────── */
    [Header("Detection Settings")]
    [Tooltip("World-space offset added to the player position before casting")]
    [SerializeField] private Vector2   originOffset    = Vector2.zero;
    [Tooltip("Detection radius in world units")]
    [SerializeField] private float     detectRadius    = 1.5f;
    [Tooltip("Layers that count as interactables")]
    [SerializeField] private LayerMask interactableMask = -1;

    [Header("Debugging")]
    [SerializeField] private bool doDebugLog = false;
    [SerializeField, InspectorReadOnly] private Interactable _focused;

    /* ────────── Internals ────────── */
    
    private bool         _focusEntered;
    private Transform    _player;

    /* ====================================================================== */
    /*                               Unity                                    */
    /* ====================================================================== */

    private void Start()
    {
        _player = transform;
        InputManager.OnInteract += OnInteractPressed;
    }

    private void OnDestroy()
    {
        InputManager.OnInteract -= OnInteractPressed;
    }

    private void Update() => DetectInteractable();

    /* ====================================================================== */
    /*                            Core Logic                                  */
    /* ====================================================================== */

    private void DetectInteractable()
    {
        Vector2 origin = (Vector2)_player.position + originOffset;

        // Grab every collider in range that is on the interactable layer
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, detectRadius, interactableMask);

        // Find the nearest Interactable (if any)
        Interactable nearest     = null;
        float        nearestSqr  = float.MaxValue;

        foreach (var c in hits)
        {
            if (!c.TryGetComponent(out Interactable i)) continue;

            float sqr = ((Vector2)c.transform.position - origin).sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearest    = i;
                nearestSqr = sqr;
            }
        }

        /* ---------- Focus change handling ---------- */
        if (nearest != _focused)
        {
            _focused?.OnFocusExit?.Invoke();
            _focused      = nearest;
            _focusEntered = false;
        }

        /* ---------- Focus life-cycle ---------- */
        if (_focused == null) return;

        if (!_focusEntered)
        {
            _focused.OnFocusEnter?.Invoke();
            _focusEntered = true;
        }
        else
        {
            _focused.OnFocusStay?.Invoke();
        }

        if (doDebugLog)
            Debug.Log($"Focused: {_focused.name}", _focused);
    }

    private void OnInteractPressed()
    {
        _focused?.OnInteractionExecuted?.Invoke();
        if (doDebugLog) Debug.Log($"Interact: {_focused.name}", _focused);
    }
    
    #if UNITY_EDITOR

    /* ====================================================================== */
    /*                                Gizmos                                  */
    /* ====================================================================== */

    /// <summary>
    /// Draws the detection circle (cyan) and highlights the current focus
    /// (green if one is found, red if none) while the object is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position + originOffset;

        // Outer detection area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, detectRadius);

        // Draw a dot showing the origin point
        Gizmos.color = new Color(0.2f, 0.6f, 1f);
        Gizmos.DrawSphere(origin, 0.05f);

        if (!Application.isPlaying) return;

        // Endpoint indicator: green for a hit, red otherwise
        Gizmos.color = _focused ? Color.green : Color.red;
        Vector2 end  = _focused ? (Vector2)_focused.transform.position
                                : origin + Vector2.up * detectRadius;
        Gizmos.DrawSphere(end, 0.08f);
    }
    
    #endif
}
