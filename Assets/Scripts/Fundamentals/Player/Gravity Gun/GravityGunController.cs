using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

/* -----------------------------------------------------------
 * Author:
 * 
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Purpose:
 * 
 */// --------------------------------------------------------


/// <summary>
/// 
/// </summary>
public class GravityGunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _gravigunPivot;
    [SerializeField] private Transform _gravigunHoldPos;
    [SerializeField] private SpriteRenderer _helperTargetCircleSprite;
    [SerializeField] private LineRenderer _lineOfSightRenderer;
    [SerializeField] private LineRenderer _holdingLineRenderer;
    
    [Header("Settings")]
    [SerializeField] private Color _defaultLineOfSightColor; // The color of the line when not pointing towards something influenceable
    [SerializeField] private Color _validTargetLineColor; // The color of the line when its pointing to a valid target
    [SerializeField, Range(0f,100f)] private float _maxRaycastDistance;
    [SerializeField] private LayerMask _lineRaycastMask;
    [SerializeField, Range(0f, 100f)] private float _pullForce;
    [SerializeField, Range(0f, 100f)] private float _maxVelocity; //
    [SerializeField, Range(0f, 100f)] private float _pushForce; 
    [SerializeField, Range(0f, 100f)] private float _pushRange; // Range in which the push works
    [SerializeField, Range(0f, 10f)] private float _grabRange; // Range where the pull grabs the object
    [SerializeField, Range(0f, 360f)] private float _rotateSpeed; // Speed at which the mousewheel rotates the object
    [SerializeField, Range(0f, 100f)] private float _grabDistanceBreak; // The distance where the grab breaks from the object
    [SerializeField, Range(0f, 2f)] private float _pullPushCooldown;
    
    [Header("Debugging")]
    [SerializeField] private bool doDebugLog;
    
    [Header("Readouts")]
    [SerializeField, InspectorReadOnly] private PhysicsObject _focusedObject;
    [SerializeField, Vector2Compass, InspectorReadOnly] private Vector2 _currentLookDir;
    [SerializeField, InspectorReadOnly] private bool _isHoldingObject;
    [SerializeField, InspectorReadOnly] private bool _pullExecutedThisFrame;
    [SerializeField, InspectorReadOnly] private bool _isPlayerHoldingPullAfterGrab;
    [SerializeField, InspectorReadOnly] private bool _isPushPullLocked;
    
    // Local variables
    private RaycastHit2D _currentHit;

    private void Update()
    {
        // Dont do anything if paused
        if (Time.timeScale <= 0) return;

        // 1. Convert mouse position to world space
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;          // flatten to sprite’s plane

        // Direction from sprite to mouse
        _currentLookDir = (mouseWorld - _gravigunPivot.position).normalized;

        // Compute angle (0° = +X). Convert to degrees.
        float angle = Mathf.Atan2(_currentLookDir.y, _currentLookDir.x) * Mathf.Rad2Deg;
        
        // 4. Rotate around Z so +Y faces the cursor
        _gravigunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // Dont do anything if holding an object
        if (_isHoldingObject)
        {
            // Disable the direction line renderer
            UpdateWhenGrabbingObject();
            _lineOfSightRenderer.gameObject.SetActive(false);
            _helperTargetCircleSprite.transform.position = _gravigunPivot.position;
            return;
        }
        
        // Wasn't holding, perform as usual
        // Fire a raycast in the direction of the mouse
        RaycastHit2D hit = Physics2D.Raycast(_gravigunPivot.position, _currentLookDir, _maxRaycastDistance, _lineRaycastMask);
        _currentHit = hit;

        if (!hit)
        {
            // we didnt hit anything
            _lineOfSightRenderer.gameObject.SetActive(false);
            ChangeTargetCircleColor(_defaultLineOfSightColor);
            ChangeLineRendererColor(_defaultLineOfSightColor);
            UpdateLineRendererPos(_gravigunPivot.position, _currentLookDir, Vector2.zero, _maxRaycastDistance);
            
            // Disable outline on the previous focused object
            if (_focusedObject)
            {
                _focusedObject.DisableTarget();
                _focusedObject = null;
            }
            return;
        }

        // We did hit something
        _lineOfSightRenderer.gameObject.SetActive(true);

        // Update Line Renderer
        UpdateLineRendererPos(_gravigunPivot.position, Vector2.zero, hit.point, 0f);

        // Get game object
        GameObject go1 = hit.collider.gameObject;

        // is it a valid target?
        if (!go1.TryGetComponent(out PhysicsObject physicsObject))
        {
            // We hit a non physics object
            ChangeTargetCircleColor(_defaultLineOfSightColor);
            ChangeLineRendererColor(_defaultLineOfSightColor);

            // Disable outline on the previous focused object
            if (_focusedObject)
            {
                _focusedObject.DisableTarget();
                _focusedObject = null;
            }

            return;
        }
        // Was of type Physics Object
        _focusedObject = physicsObject;
    }
    
    
    // Physics computation
    private void FixedUpdate()
    {
        // Dont do anything if paused
        if (Time.timeScale <= 0) return;
        
        // Dont do anything if no focused object available
        if (!_focusedObject) return;
        
        // Reset check bool
        _pullExecutedThisFrame = false;
        
        // check if ignoring
        if (_focusedObject.physicsObjectType == PhysicsObjectType.IgnoresGravigun) return;
        
        // Was an influenceable object, Enable outline and set line renderer
        // TODO: Would setting the shader property every fixed update affect performance too much?
        _focusedObject.EnableTarget();
        _focusedObject.ChangeOutlineColor(_validTargetLineColor);
        ChangeTargetCircleColor(_validTargetLineColor);
        ChangeLineRendererColor(_validTargetLineColor);
        
        // Move focused object if grabbing
        if (_isHoldingObject)
        {
            // Move object to destination
            _focusedObject.rb.MovePosition(_gravigunHoldPos.position);
        }
        
        // Dont do anything if on cooldown
        if (_isPushPullLocked) return;
        
        // Check if we are grabbing
        if (_isHoldingObject)
        {
            // Dont accept input until the player releases pull at least once
            if (_isPlayerHoldingPullAfterGrab) return;
            
            // Check if player pressed pull as to drop the object
            if (InputManager.Instance.pullHeldDownInput)
            {
                // Stop holding object
                _isHoldingObject = false;
                _focusedObject.rb.freezeRotation = false;
            
                // Trigger Cooldown
                StartCoroutine(LockPullPushRoutine(_pullPushCooldown));
                return;
            }
        
            // Check if the player clicked push as to launch the object
            if (InputManager.Instance.PopPushInputRecieved())
            {
                // Stop holding object
                _isHoldingObject = false;
                _focusedObject.rb.freezeRotation = false;
            
                // Perform push
                PerformPush();
            
                // Trigger cooldown
                StartCoroutine(LockPullPushRoutine(_pullPushCooldown));
                return;
            }
            return;
        }
        
        // Perform Pull on non grabbed object if set to do so
        if (InputManager.Instance.pullHeldDownInput && !_isHoldingObject)
        {
            PerformPull();
            _pullExecutedThisFrame = true;
        }
        
        // Perform push on non grabbed object if set to do so, but ignore if pull is being held down
        if (!InputManager.Instance.pullHeldDownInput && !_isHoldingObject && InputManager.Instance.PopPushInputRecieved() && !_pullExecutedThisFrame)
        {
            // Check if in range
            if (Vector2.Distance(_currentHit.point, _gravigunPivot.position) > _pushRange) return; // not in range
            
            // Was within range
            PerformPush();
            
            // Cooldown
            StartCoroutine(LockPullPushRoutine(_pullPushCooldown));
        }
        
        // passed both pull and push, reset bool
        _pullExecutedThisFrame = false;
        
        // Dont attempt to grab object if not of type grabbable
        if (_focusedObject.physicsObjectType != PhysicsObjectType.Grabbable) return;
        
        // Dont grab object if not in range
        if (Vector2.Distance(_focusedObject.transform.position, _gravigunHoldPos.position) >
            _grabRange) return;
        
        // Dont grab if not pulling
        if (!InputManager.Instance.pullHeldDownInput) return;
        
        // Grab object
        _isHoldingObject = true;
        _isPlayerHoldingPullAfterGrab = true;
        _focusedObject.rb.freezeRotation = true;
        
        // Wait for the player to release pull before doing anything
        StartCoroutine(WaitForPullReleaseRoutine());
    }

    /// <summary>
    /// Lock the grab for a bit to avoid input overlap
    /// </summary>
    /// <returns></returns>
    private IEnumerator LockPullPushRoutine(float time)
    {
        _isPushPullLocked = true;
        yield return new WaitForSeconds(time);
        _isPushPullLocked = false;
    }

    /// <summary>
    /// Coroutine that waits until the player lets go of pull to execute more actions
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForPullReleaseRoutine()
    {
        while (_isPlayerHoldingPullAfterGrab)
        {
            _isPlayerHoldingPullAfterGrab = InputManager.Instance.pullHeldDownInput;
            yield return null;
        }
    }

    /// <summary>
    /// Function executed whenever the player is grabbing an object inside Update
    /// </summary>
    private void UpdateWhenGrabbingObject()
    {
        // Check for mouse up and down for rotation
        if (InputManager.Instance.didPlayerRotateFoward)
        {
            _focusedObject.transform.Rotate(Vector3.forward, _rotateSpeed * Time.deltaTime, Space.Self);
        }
        else if (InputManager.Instance.didPlayerRotateBackwards)
        {
            _focusedObject.transform.Rotate(Vector3.forward, -1 * _rotateSpeed * Time.deltaTime, Space.Self);
        }
    }
    
    /// <summary>
    /// Performs a pull on the currently focused object
    /// </summary>
    private void PerformPull()
    {
        // Apply force
        Vector2 dir = (_gravigunHoldPos.position - _focusedObject.transform.position);
        ApplyCappedForce(_focusedObject.rb,
            dir,
            _pullForce,
            ForceMode2D.Force,
            _maxVelocity);
    }
    
    /// <summary>
    /// Performs a push on the currently focused object
    /// </summary>
    private void PerformPush()
    {
        _focusedObject.rb.AddForce(_currentLookDir * _pushForce, ForceMode2D.Impulse);
    }
    
    /// <summary>
    /// Adds force towards target without going over the velocity cap
    /// </summary>
    private void ApplyCappedForce(Rigidbody2D rb, Vector2 dir, float force, ForceMode2D mode, float maxVel)
    {
        dir = dir.normalized;
        Vector2 v = rb.linearVelocity;
        float along = Vector2.Dot(v, dir); // signed speed toward dir

        // accelerate only while below the cap
        if (along < maxVel) rb.AddForce(dir * force, mode);
    }
    
    
    // REMEMBER TO DISABLE LINE RENDERER WHEN HOLDING SOMETHING

    private void UpdateLineRendererPos(Vector2 origin, Vector2 dir, Vector2 target, float distance)
    {
        _lineOfSightRenderer.SetPosition(0, origin);
        Vector2 finalPos = Vector2.zero;
        
        if (dir == Vector2.zero)
        {
            // We do have a target
            finalPos = target;
            _lineOfSightRenderer.SetPosition(1, target);
        }
        else
        {
            // We dont have a target
            finalPos = dir * distance;
            _lineOfSightRenderer.SetPosition(1, finalPos);
        }
        
        // Update helper circle
        _helperTargetCircleSprite.transform.position = finalPos;
    }

    private void ChangeLineRendererColor(Color color)
    {
        _lineOfSightRenderer.startColor = color;
        _lineOfSightRenderer.endColor = color;
    }

    private void ChangeTargetCircleColor(Color color)
    {
        _helperTargetCircleSprite.color = color;
    }
    
    #if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        
    }
    
    
    #endif
    
}
