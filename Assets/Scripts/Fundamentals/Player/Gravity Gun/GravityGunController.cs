using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

// WARNING: This script is very complex, I highly suggest you
// come and ask me before adding or modifying anything on it.
// Cuz honestly only me and god knows how it works

/// <summary>
/// Class that manages the player's gravity gun
/// </summary>
public class GravityGunController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GravityGunSettings _settings;
    
    [Header("References")]
    [SerializeField] private Transform _gravigunPivot;
    [SerializeField] private Transform _gravigunHoldPosDynamic;
    [SerializeField] private Transform _gravigunHoldPosStatic;
    [SerializeField] private SpriteRenderer _helperTargetCircleSprite;
    [SerializeField] private SpriteRenderer _gravigunSpriteRenderer;
    [SerializeField] private LineRenderer _lineOfSightRenderer;
    [SerializeField] private LineRenderer _bezierLineOneRenderer;
    [SerializeField] private LineRenderer _bezierLineTwoRenderer;
    [SerializeField] private LineRenderer _holdLineRenderer;
    
    [Header("Gravigun Sprites")]
    [SerializeField] private Sprite _gravigunSpriteOn;
    [SerializeField] private Sprite _gravigunSpriteOff;

    [Header("Sounds")] 
    [SerializeField] private SimpleAudioEmitter _gravigunLaunch;
    [SerializeField] private SimpleAudioEmitter _gravigunGrab;
    [SerializeField] private SimpleAudioEmitter _gravigunDrop;
    [SerializeField] private SimpleAudioEmitter _gravigunNoTarget;
    [SerializeField] private SimpleAudioEmitter _gravigunPull;
    [SerializeField] private SimpleAudioEmitter _gravigunTooHeavy;
    
    [Header("Others")]
    [SerializeField] private float _standingOnCheckBoxHeight;
    
    [Header("Debugging")]
    [SerializeField] private bool doDebugLog;
    
    [Header("Readouts")]
    [SerializeField, InspectorReadOnly] private PhysicsObject _focusedObject;
    [SerializeField, Vector2Compass, InspectorReadOnly] private Vector2 _currentLookDir;
    [SerializeField, InlineToggle, InspectorReadOnly] private bool _isHoldingObject;
    [SerializeField, InlineToggle, InspectorReadOnly] private bool _pullExecutedThisFrame;
    [SerializeField, InlineToggle, InspectorReadOnly] private bool _isPlayerHoldingPullAfterGrab;
    [SerializeField, InlineToggle, InspectorReadOnly] private bool _isPushPullLocked;
    [SerializeField, InlineToggle, InspectorReadOnly] private bool _isInBetweenHoldAndCenter;
    [SerializeField, InlineToggle, InspectorReadOnly] private bool _isNearHoldPos;
    [SerializeField, InlineToggle, InspectorReadOnly] private bool _isGravityGunOff;
    
    
    // Local variables
    private RaycastHit2D _currentHit;
    private List<PhysicsObject> _trackedObjects = new();
    private Coroutine _fadeHoldLinesCo;
    private bool _dontPlayPullSound; // Helper bool for sound playing

    private void Awake()
    {
        InputManager.OnGravityGunToggle += ToggleGravigun;
    }

    private void OnDestroy()
    {
        InputManager.OnGravityGunToggle -= ToggleGravigun;
    }

    private void Update()
    {
        // Dont do anything if paused
        if (Time.timeScale <= 0) return;
        
        // Check if there's a camera tagged main
        if (!Camera.main)
        {
            Debug.LogError("ERROR! There is no camera tagged MainCamera on the scene!");
            return;
        }

        UpdateAimAndPivot();
        
        // Dont do anything if turned off
        if (_isGravityGunOff)
        {
            if (_focusedObject)
            {
                StopHoldingObject();
                _focusedObject = null;
            }
            _lineOfSightRenderer.gameObject.SetActive(false);
            _helperTargetCircleSprite.gameObject.SetActive(false);
            return;
        }
        _helperTargetCircleSprite.gameObject.SetActive(true);

        // Handle Mouse Wheel Click
        if (InputManager.Instance.didPlayerClickMouseWheelThisFrame)
        {
            HandleMouseWheelClick();
        }
        
        if (_isHoldingObject)
        {
            UpdateWhenGrabbingObject();
        }
        else
        {
            UpdateWhenNotHoldingObject();
        }
        
        HandlePhysicsAndInput();
    }

    /// <summary>
    /// Computes and rotates the gravity gun and its direction
    /// </summary>
    private void UpdateAimAndPivot()
    {
        // 1. Convert mouse position to world space
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;          // flatten to sprite’s plane

        // Direction from sprite to mouse
        _currentLookDir = (mouseWorld - _gravigunPivot.position).normalized;

        // Compute angle (0° = +X). Convert to degrees.
        float angle = Mathf.Atan2(_currentLookDir.y, _currentLookDir.x) * Mathf.Rad2Deg;
        
        // 4. Rotate around Z so +Y faces the cursor
        _gravigunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    /// <summary>
    /// Update loop when not holding an object
    /// </summary>
    private void UpdateWhenNotHoldingObject()
    {
        // Wasn't holding, perform as usual
        // Fire a raycast in the direction of the mouse
        RaycastHit2D hit = Physics2D.Raycast(_gravigunPivot.position, _currentLookDir, _settings.maxRaycastDistance, _settings.lineRaycastMask);
        _currentHit = hit;

        if (!hit)
        {
            // we didnt hit anything
            _lineOfSightRenderer.gameObject.SetActive(false);
            ChangeTargetCircleColor(_settings.defaultLineOfSightColor);
            ChangeLineRendererColor(_settings.defaultLineOfSightColor);
            UpdateLineRendererPos(_gravigunPivot.position, _currentLookDir, Vector2.zero, _settings.maxRaycastDistance);
            
            // If the player clicked push or pull, play sound for no target
            if (InputManager.Instance.pullPressedThisFrame || InputManager.Instance.pushPressedThisFrame)
            {
                _gravigunNoTarget.PlaySound();
            }
            
            // Clear out focused field
            if (_focusedObject) _focusedObject = null;
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
            ChangeTargetCircleColor(_settings.defaultLineOfSightColor);
            ChangeLineRendererColor(_settings.defaultLineOfSightColor);
            
            // If the player performs pull or push on it, play no target sound
            if (InputManager.Instance.pullPressedThisFrame || InputManager.Instance.pushPressedThisFrame)
            {
                _gravigunNoTarget.PlaySound();
            }

            // Clear out focused field
            if (_focusedObject) _focusedObject = null;
            return;
        }
        
        // Was of type Physics Object
        
        // Keep a reference
        _focusedObject = physicsObject;
        
        // Check if ignoring
        if (physicsObject.physicsObjectType == PhysicsObjectType.IgnoresGravigun) return;
        
        // Check mass limit
        bool isObjectTooHeavy = _focusedObject.rb.mass > _settings.maxMass;
        
        // Perform push on non grabbed object if set to do so, but ignore if pull is being held down
        bool inRange = Vector2.Distance(_currentHit.point, _gravigunPivot.position) < _settings.pushRange;
        bool pushInputCheck = InputManager.Instance.pushPressedThisFrame && !InputManager.Instance.pullHeldDownInput;
        if (!isObjectTooHeavy && pushInputCheck && !_isHoldingObject && inRange)
        {
            // Was within range and can push
            PerformPush();

            // Cooldown
            StartCoroutine(LockPullPushRoutine(_settings.pullPushCooldown));
        }
        
        // Check if above mass limit and in range to play too heavy sound on push, otherwise no target
        if (inRange && InputManager.Instance.pushPressedThisFrame && isObjectTooHeavy)
        {
            _gravigunTooHeavy.PlaySound();
        }
        else if (InputManager.Instance.pushPressedThisFrame)
        {
            // Nothing in range, play no target sound
            _gravigunNoTarget.PlaySound();
        }
        
        // Check if above mass limit to play too heavy sound on pull
        if (isObjectTooHeavy && InputManager.Instance.pullPressedThisFrame)
        {
            // We play the too heavy to pull sound
            _gravigunTooHeavy.PlaySound();
        }
    }

    /// <summary>
    /// Function executed whenever the player is grabbing an object inside Update
    /// </summary>
    private void UpdateWhenGrabbingObject()
    {
        // Disable the direction line renderer
        _lineOfSightRenderer.gameObject.SetActive(false);
        _helperTargetCircleSprite.transform.position = _focusedObject.transform.position;

        // Activate bezier lines
        _bezierLineOneRenderer.enabled = true;
        _bezierLineTwoRenderer.enabled = true;
        _holdLineRenderer.enabled = true;
        
        // Check if we should change the hold color depending on push range
        if (Vector2.Distance(_gravigunHoldPosDynamic.position, _gravigunHoldPosStatic.position) > _settings.pushRange)
        {
            ChangeTargetCircleColor(_settings.validTargetLineColor);
        }
        else
        {
            ChangeTargetCircleColor(_settings.canPushColor);
        }
        
        // Check for mouse up and down for rotation
        if (InputManager.Instance.didPlayerWheelFoward)
        {
            // Check if we are rotating
            if (InputManager.Instance.didPlayerHoldDownRotate)
            {
                _focusedObject.transform.Rotate(Vector3.forward, _settings.rotateSpeed * Time.deltaTime, Space.Self);
                return;
            }
            
            // move object foward only if not passing max distance
            if (Vector2.Distance(_gravigunHoldPosDynamic.position, _gravigunHoldPosStatic.position) >= _settings.WheelMoveMaxDistance) return;
            
            // Otherwise, move object foward
            _gravigunHoldPosDynamic.Translate( _settings.WheelMoveSpeed * Time.deltaTime * _currentLookDir, Space.World);
            
        }
        else if (InputManager.Instance.didPlayerWheelBackwards)
        {
            // Check if we are rotating backwards
            if (InputManager.Instance.didPlayerHoldDownRotate)
            {
                _focusedObject.transform.Rotate(Vector3.forward, -1 * _settings.rotateSpeed * Time.deltaTime, Space.Self);
                return;
            }
            
            // Check for negatives in the local dynamic transform
            if (_gravigunHoldPosDynamic.localPosition.x < 0f || _gravigunHoldPosDynamic.localPosition.y < 0f)
            {
                float absXval = Mathf.Abs(_gravigunHoldPosDynamic.localPosition.x);
                float absYval = Mathf.Abs(_gravigunHoldPosDynamic.localPosition.y);
                Vector2 newPos = new Vector2(absXval, absYval);
                _gravigunHoldPosDynamic.localPosition = newPos;
            }
            
            // move object backwards only if not behind the static starting position
            if (_gravigunHoldPosDynamic.localPosition is { x: >= 0f, y: >= 0f })
            {
                _gravigunHoldPosDynamic.Translate( -1 * _settings.WheelMoveSpeed * Time.deltaTime * _currentLookDir, Space.World);
            }
        }
        
        // If the player clicked the mouse wheel and we have a special object grabbed, trigger it
        // Invoke events if of type special object
        if (InputManager.Instance.didPlayerClickMouseWheelThisFrame && _focusedObject.physicsObjectType is PhysicsObjectType.Special)
        {
            GravSpecialObject gsp = (GravSpecialObject)_focusedObject;
            gsp.gravEvents.onGravityGunSpecialTriggered.Invoke();
        }
    }

    /// <summary>
    /// Handles all physics and input logic
    /// </summary>
    private void HandlePhysicsAndInput()
    {
        // Dont do anything if no focused object available
        if (!_focusedObject) return;
        
        // Check mass limit
        bool isObjectTooHeavy = _focusedObject.rb.mass > _settings.maxMass;
        
        // Reset check bool
        _pullExecutedThisFrame = false;
        
        // check if ignoring
        if (_focusedObject.physicsObjectType == PhysicsObjectType.IgnoresGravigun)
        {
            _focusedObject.ChangeOutlineColor(_settings.defaultLineOfSightColor);
            ChangeTargetCircleColor(_settings.defaultLineOfSightColor);
            ChangeLineRendererColor(_settings.defaultLineOfSightColor);
            return;
        }
        
        // Was an influenceable object, Enable outline and set line renderer
        if (!_trackedObjects.Contains(_focusedObject))
        {
            _focusedObject.EnableTarget();
            _focusedObject.ChangeOutlineColor(_settings.validTargetLineColor);
            ChangeTargetCircleColor(_settings.validTargetLineColor);
            if (isObjectTooHeavy)
            {
                _focusedObject.ChangeOutlineColor(_settings.tooHeavyColor);
                ChangeTargetCircleColor(_settings.tooHeavyColor);
                ChangeLineRendererColor(_settings.tooHeavyColor);
            }
            StartCoroutine(TrackFocusedObjectLeftRoutine(_focusedObject));
        }
        
        // Dont do anything if object is over the mass limit
        if (isObjectTooHeavy) return;

        // Move focused object if grabbing
        if (_isHoldingObject)
        {
            // If the player stands on top of the object, break hold
            if (IsPlayerStandingOn(_focusedObject))
            {
                StopHoldingObject();
                // Trigger a longer cooldown routine to avoid prop surfing
                StartCoroutine(LockPullPushRoutine(1f));
            }
            else
            {
                // Move object
                MoveFocusedObject();
            }
        }
        
        // Dont do anything if on cooldown
        if (_isPushPullLocked) return;
        
        // Check if we are grabbing
        if (_isHoldingObject)
        {
            // Start coroutine
            if (_fadeHoldLinesCo == null) _fadeHoldLinesCo = StartCoroutine(FadeHoldLinesRoutine());
            
            // Break if the object goes too far from hold pos
            if (Vector2.Distance(_focusedObject.transform.position, _gravigunHoldPosDynamic.position) > _settings.grabDistanceBreak)
            {
                // Stop holding object
                StopHoldingObject();
                
                // Trigger Cooldown
                StartCoroutine(LockPullPushRoutine(_settings.pullPushCooldown));
                return;
            }
            
            // Dont accept input until the player releases pull at least once
            if (_isPlayerHoldingPullAfterGrab) return;
            
            // Check if player pressed pull as to drop the object
            if (InputManager.Instance.pullHeldDownInput)
            {
                // Stop holding object
                StopHoldingObject();
                
                // Trigger Cooldown
                StartCoroutine(LockPullPushRoutine(_settings.pullPushCooldown));
                return;
            }
        
            // Check if in range of pushing
            bool inRange = Vector2.Distance(_focusedObject.transform.position, _gravigunPivot.position) < _settings.pushRange;
            if (inRange)
            {
                ChangeBezierRendererColor(_settings.canPushColor);
                if (_focusedObject) _focusedObject.ChangeOutlineColor(_settings.canPushColor);
            }
            else
            {
                ChangeBezierRendererColor(_settings.validTargetLineColor);
                if (_focusedObject) _focusedObject.ChangeOutlineColor(_settings.validTargetLineColor);
            }
            // Check if the player clicked push as to launch the object
            if (InputManager.Instance.pushPressedThisFrame && inRange)
            {
                // Stop holding object
                StopHoldingObject();
                
                // Perform push
                PerformPush();
                
                // Trigger cooldown
                StartCoroutine(LockPullPushRoutine(_settings.pullPushCooldown));
                return;
            }
            return;
        }
        
        // Perform Pull on non grabbed object if set to do so
        if (InputManager.Instance.pullHeldDownInput && !_isHoldingObject)
        {
            PerformPull();
            // Trigger a longer cooldown routine to avoid prop surfing
            if (IsPlayerStandingOn(_focusedObject)) StartCoroutine(LockPullPushRoutine(1f));
            _pullExecutedThisFrame = true;
        }
        else
        {
            // Refresh the state of the sound bool if not pulling
            _dontPlayPullSound = false;
        }
        
        // Check if in range of push
        bool inRange2 = Vector2.Distance(_currentHit.point, _gravigunPivot.position) < _settings.pushRange;
        if (inRange2)
        {
            // Change line renderer color as to show object can be pushed, the same with its outline
            ChangeLineRendererColor(_settings.canPushColor);
            ChangeTargetCircleColor(_settings.canPushColor);
            if (_focusedObject) _focusedObject.ChangeOutlineColor(_settings.canPushColor);
        }
        else
        {
            // Default colors
            ChangeLineRendererColor(_settings.validTargetLineColor);
            if (_focusedObject) _focusedObject.ChangeOutlineColor(_settings.validTargetLineColor);
        }
        
        // passed both pull and push, reset bool
        _pullExecutedThisFrame = false;
        
        // Dont attempt to grab object if not of type grabbable or special
        bool isGrabbable = _focusedObject.physicsObjectType is PhysicsObjectType.Grabbable or PhysicsObjectType.Special;
        if (!isGrabbable) return;

        _isNearHoldPos = Vector2.Distance(_focusedObject.transform.position, _gravigunHoldPosDynamic.position) <
                         _settings.grabRange;
        
        _isInBetweenHoldAndCenter = InBetweenXAxis(_focusedObject.transform.position, _gravigunPivot.position, _gravigunHoldPosDynamic.position);
        
        // Dont grab object if not in range or if not in between the hold pos and the player
        if (!_isNearHoldPos && !_isInBetweenHoldAndCenter) return;
        
        // Dont grab if not pulling
        if (!InputManager.Instance.pullHeldDownInput) return;
        
        // Prevent grabbing if the player is standing on the object
        if (IsPlayerStandingOn(_focusedObject)) return;

        // Grab object
        GrabFocusedObject();
        
        // Wait for the player to release pull before doing anything
        StartCoroutine(WaitForPullReleaseRoutine());
    }

    /// <summary>
    /// Helper function that will move the object towards destination
    /// </summary>
    private void MoveFocusedObject()
    {
        // where we want the object to end up
        Vector3 target = _gravigunHoldPosDynamic.position;
        Vector3 current = _focusedObject.rb.position;
            
        Vector3 toTarget = target - current;
        float distance = toTarget.magnitude;
        
        // Check if we are close enough
        if (distance < 0.02f)
        {
            _focusedObject.rb.MovePosition(target); // snap & finish
            return;
        }
        
        // distance‑scaled speed
        // speed = baseSpeed * (distance^strength)
        float speed = _settings.focusedMoveBaseSpeed * Mathf.Pow(distance, _settings.focusedMoveStrengthExponent);
        speed       = Mathf.Min(speed, _settings.focusedMoveMaxSpeed);     // clamp
    
        // Move object to destination
        Vector3 step = speed * Time.deltaTime * toTarget.normalized; 
        _focusedObject.rb.MovePosition(current + step);
    }

    /// <summary>
    /// Function that handles anything regarding the mouse wheel click
    /// </summary>
    private void HandleMouseWheelClick()
    {
        
    }
    
    /// <summary>
    /// Checks if the player is standing on top of the object
    /// </summary>
    private bool IsPlayerStandingOn(PhysicsObject obj)
    {
        Vector2 localScale = _focusedObject.transform.localScale;
        Vector2 checkBoxCenter = (Vector2)_focusedObject.transform.position + Vector2.up * (_standingOnCheckBoxHeight/2 - localScale.y/3);
        Vector2 checkBoxSize = new Vector2(localScale.x, _standingOnCheckBoxHeight);

        Collider2D[] hits = Physics2D.OverlapBoxAll(checkBoxCenter, checkBoxSize, 0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player")) return true;
        }

        return false;
    }

    /// <summary>
    /// Function called whenever we want to grab the current focused object
    /// </summary>
    private void GrabFocusedObject()
    {
        BombLineRendererPoints();
        _gravigunGrab.PlaySound();
        _isHoldingObject = true;
        _isPlayerHoldingPullAfterGrab = true;
        _focusedObject.rb.freezeRotation = true;
        
        // Invoke events if of type special object
        if (_focusedObject.physicsObjectType is PhysicsObjectType.Special)
        {
            GravSpecialObject gsp = (GravSpecialObject)_focusedObject;
            gsp.gravEvents.OnGravityGunGrab.Invoke();
        }
    }

    /// <summary>
    /// Function called to stop holding the current object
    /// </summary>
    private void StopHoldingObject()
    {
        _gravigunDrop.PlaySound();
        // Invoke events if of type special object
        if (_focusedObject.physicsObjectType is PhysicsObjectType.Special)
        {
            GravSpecialObject gsp = (GravSpecialObject)_focusedObject;
            gsp.gravEvents.OnGravityGunDrop.Invoke();
        }
        _isHoldingObject = false;
        _focusedObject.rb.freezeRotation = false;
        _focusedObject.rb.linearVelocity = Vector2.zero;
        _bezierLineOneRenderer.enabled = false;
        _bezierLineTwoRenderer.enabled = false;
        _holdLineRenderer.enabled = false;
        _gravigunHoldPosDynamic.position = _gravigunHoldPosStatic.position;
    }

    /// <summary>
    /// Function used to avoid visual bugs when triggering the hold lines
    /// </summary>
    private void BombLineRendererPoints()
    {
        // Create a zero array to zero out the points
        Vector3[] points = Array.Empty<Vector3>();
        
        // Begin
        _lineOfSightRenderer.SetPositions(points);
        _bezierLineOneRenderer.SetPositions(points);
        _bezierLineTwoRenderer.SetPositions(points);
        _holdLineRenderer.SetPositions(points);
    }

    /// <summary>
    /// Coroutine that handles the fading of the lines in case an object blocks the path of the held object and the player
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeHoldLinesRoutine()
    {
        // Timer
        float t = 0f;
        
        // Loop
        while (_isHoldingObject)
        {
            // Timer
            t += Time.deltaTime;
            
            // Check if we have passed the break limit
            if (t >= _settings.blockingObjectTimeBreak)
            {
                // we did pass the limit, break hold
                StopHoldingObject();
                break;
            }
            
            // Math
            float a = Mathf.Lerp(1f, 0f, t / _settings.blockingObjectTimeBreak);
            
            // Perform a raycast
            float distance = Vector2.Distance(_gravigunPivot.position, _focusedObject.transform.position);
            Vector2 dir =  (_focusedObject.transform.position - _gravigunPivot.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(_gravigunPivot.position, dir, distance, _settings.lineRaycastMask);
            
            // Get game object
            GameObject go1 = hit.collider.gameObject;

            // Check if we hit the focused object
            if (!go1.TryGetComponent(out PhysicsObject physicsObject))
            {
                // We hit a non physics object, meaning sight got broken
                SetAlphaAllColors(a);
                yield return null;
                continue;
            }
            
            // We hit a physics object, check if its the same as the focused one
            if (physicsObject != _focusedObject)
            {
                // Was different, line of sight was broken
                SetAlphaAllColors(a);
                yield return null;
                continue;
            }
            
            // We did hit the focused object, keep holding
            t = 0;
            SetAlphaAllColors(1f);
            yield return null;
        }
        
        // Not holding an object anymore
        SetAlphaAllColors(1f);
        _fadeHoldLinesCo = null;
    }
    
    /// <summary>
    /// Function to help the FadeHoldLinesRoutine as to fade lines as the sight breaks
    /// </summary>
    /// <param name="alpha"> Alpha value between 1 and 0 </param>
    private void SetAlphaAllColors(float alpha)
    {
        _settings.defaultLineOfSightColor.a = alpha;
        _settings.validTargetLineColor.a = alpha;
        _settings.canPushColor.a = alpha;
    }
    
    /// <summary>
    /// Function that toggles the gravity gun on and off
    /// </summary>
    private void ToggleGravigun()
    {
        // Toggle
        _isGravityGunOff = !_isGravityGunOff;
        _gravigunSpriteRenderer.sprite = _isGravityGunOff ? _gravigunSpriteOff : _gravigunSpriteOn;
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
    /// Coroutine meant to handle the disabling of the outline of target physics objects
    /// </summary>
    /// <param name="physicsObject"> The physics object to track </param>
    /// <returns></returns>
    private IEnumerator TrackFocusedObjectLeftRoutine(PhysicsObject physicsObject)
    {
        // Add physics object to list of tracked object
        _trackedObjects.Add(physicsObject);
        _focusedObject.EnableTarget();
        
        // Wait until the focused object changes
        while (_focusedObject == physicsObject && physicsObject.physicsObjectType != PhysicsObjectType.IgnoresGravigun)
        {
            yield return null;
        }
        // It changed, remove outline and shii
        _trackedObjects.Remove(physicsObject);
        physicsObject.DisableTarget();
    }
    
    /// <summary>
    /// Performs a pull on the currently focused object
    /// </summary>
    private void PerformPull()
    {
        // Dont perform pull if standing on object
        if (IsPlayerStandingOn(_focusedObject)) return;
        
        // Play sound and event
        if (!_dontPlayPullSound)
        {
            _gravigunPull.PlaySound();
            // Invoke events if of type special object
            if (_focusedObject.physicsObjectType is PhysicsObjectType.Special)
            {
                GravSpecialObject gsp = (GravSpecialObject)_focusedObject;
                gsp.gravEvents.OnGravityGunPull.Invoke();
            }
            _dontPlayPullSound = true;
        }
        
        Vector2 toTarget = _gravigunHoldPosDynamic.position - _focusedObject.transform.position;
        float distance = toTarget.magnitude;
        
        // Apply force
        // Curve-based pull scaling (soft near, strong far)
        float rampedForce = _settings.pullForce * Mathf.Pow(distance, _settings.focusedMoveStrengthExponent);
        rampedForce = Mathf.Min(rampedForce, _settings.pullForce); // Optional: clamp to original pullForce

        ApplyCappedForce(_focusedObject.rb, toTarget, rampedForce, ForceMode2D.Force, _settings.maxVelocity);
    }
    
    /// <summary>
    /// Performs a push on the currently focused object
    /// </summary>
    private void PerformPush()
    {
        _gravigunLaunch.PlaySound();
        // Invoke events if of type special object
        if (_focusedObject.physicsObjectType is PhysicsObjectType.Special)
        {
            GravSpecialObject gsp = (GravSpecialObject)_focusedObject;
            gsp.gravEvents.OnGravityGunLaunch.Invoke();
        }
        _focusedObject.rb.AddForce(_currentLookDir * _settings.pushForce, ForceMode2D.Impulse);
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

    /// <summary>
    /// Returns true when target.x lies between point1.x and point2.x (inclusive).
    /// Works no matter which point is left or right.
    /// </summary>
    private bool InBetweenXAxis(Vector2 target, Vector2 point1, Vector2 point2)
    {
        float minX = Mathf.Min(point1.x, point2.x);
        float maxX = Mathf.Max(point1.x, point2.x);
        return target.x >= minX && target.x <= maxX;
    }

    /// <summary>
    /// Helper method that will change the color of the bezier hold lines
    /// </summary>
    /// <param name="color"> The target color to set </param>
    private void ChangeBezierRendererColor(Color color)
    {
        _bezierLineOneRenderer.startColor = color;
        _bezierLineOneRenderer.endColor = color;
        _bezierLineTwoRenderer.startColor = color;
        _bezierLineTwoRenderer.endColor = color;
        _holdLineRenderer.startColor = color;
        _holdLineRenderer.endColor = color;
    }
    
    #if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (_focusedObject != null)
        {
            Gizmos.color = Color.red;
            Vector2 localScale = _focusedObject.transform.localScale;
            Vector2 checkBoxCenter = (Vector2)_focusedObject.transform.position + Vector2.up * (_standingOnCheckBoxHeight/2 - localScale.y/3);
            Vector2 checkBoxSize = new Vector2(localScale.x, _standingOnCheckBoxHeight);
            Gizmos.DrawWireCube(checkBoxCenter, checkBoxSize);
        }
    }
    
    
    #endif
    
}
