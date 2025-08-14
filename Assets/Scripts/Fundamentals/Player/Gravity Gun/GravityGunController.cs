using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * Cami Lee
 * 
 */// --------------------------------------------------------

// WARNING: This script is very complex, I highly suggest you
// come and ask me before adding or modifying anything on it.
// Cuz honestly only me and god knows how it works
// --> Note by Cami: The script has been modified to be less complex;
// however, there are still some complexities present and caution
// should be taken with editing it

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
    [SerializeField] private GameObject player;

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
    bool isPushing;
    bool isPulling;
    bool isRotating;

    enum LineState { NoObject, TooHeavy, ValidObject, GrabbingTooFar, GrabbingNormal, Off, InvalidObject, ObjectIgnoresGravigun }

    LineState currentState = LineState.NoObject;

    #region State Logic

    private void OffState()
    {
        if (_focusedObject)
        {
            StopHoldingObject();
            _focusedObject = null;
        }
        _lineOfSightRenderer.gameObject.SetActive(false);
        _helperTargetCircleSprite.gameObject.SetActive(false);

        _gravigunSpriteRenderer.sprite = _isGravityGunOff ? _gravigunSpriteOff : _gravigunSpriteOn;
    }

    private void HeavyState()
    {
        _focusedObject.EnableTarget();
        _focusedObject.ChangeOutlineColor(_settings.tooHeavyColor);

        UpdateLineRenderer(true, _settings.tooHeavyColor);
        UpdateTargetCircle(_settings.tooHeavyColor);
    }

    private void NoObjectState()
    {
        // we didn't hit anything
        UpdateLineRenderer(false, _settings.defaultLineOfSightColor);
        UpdateTargetCircle(_settings.defaultLineOfSightColor);
        UpdateLineRendererPos(_gravigunPivot.position, _currentLookDir, Vector2.zero, _settings.maxRaycastDistance);

        // Clear out focused field
        if (_focusedObject) _focusedObject = null;
    }

    private void InvalidObjectState(RaycastHit2D hit)
    {
        // We hit a non physics object
        UpdateLineRenderer(true, _settings.defaultLineOfSightColor);
        UpdateTargetCircle(_settings.defaultLineOfSightColor);
        UpdateLineRendererPos(_gravigunPivot.position, Vector2.zero, hit.point, 0f);

        // Clear out focused field
        if (_focusedObject) _focusedObject = null;
    }

    private void IgnoreGravigunState()
    {
        _focusedObject.ChangeOutlineColor(_settings.defaultLineOfSightColor);
        UpdateLineRenderer(true, _settings.defaultLineOfSightColor);
        UpdateTargetCircle(_settings.defaultLineOfSightColor);
    }

    private void ValidObjectState(RaycastHit2D hit)
    {
        UpdateLineRenderer(true, _settings.validTargetLineColor);
        UpdateTargetCircle(_settings.validTargetLineColor);
        UpdateLineRendererPos(_gravigunPivot.position, Vector2.zero, hit.point, 0f);

        _focusedObject.ChangeOutlineColor(_settings.validTargetLineColor);
        StartCoroutine(TrackFocusedObjectLeftRoutine(_focusedObject));
    }

    #endregion

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

        DetermineStates();
    }

    #region Focus Logic

    /// <summary> Function that toggles the gravity gun on and off </summary>
    private void OnToggle()
    {
        currentState = LineState.Off;
    }

    /// <summary> Computes and rotates the gravity gun and its direction </summary>
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

    /// <summary> Helper function that will move the object towards destination </summary>
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
        speed = Mathf.Min(speed, _settings.focusedMoveMaxSpeed);     // clamp

        // Move object to destination
        Vector3 step = speed * Time.deltaTime * toTarget.normalized;
        _focusedObject.rb.MovePosition(current + step);
    }

    /// <summary> Function called whenever we want to grab the current focused object </summary>
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

        currentState = LineState.GrabbingNormal;
    }


    /// <summary> Coroutine meant to handle the disabling of the outline of target physics objects </summary>
    /// <param name="physicsObject"> The physics object to track </param>
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

    #endregion

    #region Holding Logic

    /// <summary> Function called to stop holding the current object </summary>
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

    private void OnMouseWheelUp()
    {
        if (_isHoldingObject)
        {
            if (isRotating)
            {
                // Check if we are rotating backwards
                _focusedObject.transform.Rotate(Vector3.forward, -1 * _settings.rotateSpeed * Time.deltaTime, Space.Self);
                return;
            }

            // move object foward only if not passing max distance
            if (Vector2.Distance(_gravigunHoldPosDynamic.position, _gravigunHoldPosStatic.position) >= _settings.WheelMoveMaxDistance) return;

            // Otherwise, move object foward
            _gravigunHoldPosDynamic.Translate(_settings.WheelMoveSpeed * Time.deltaTime * _currentLookDir, Space.World);
        }
    }

    private void OnMouseWheelDown()
    {
        if (_isHoldingObject)
        {
            if (isRotating)
            {
                _focusedObject.transform.Rotate(Vector3.forward, 1 * _settings.rotateSpeed * Time.deltaTime, Space.Self);
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
                _gravigunHoldPosDynamic.Translate(-1 * _settings.WheelMoveSpeed * Time.deltaTime * _currentLookDir, Space.World);
            }
        }
    }

    private void OnRotate(InputValue value)
    {
        isRotating = value.isPressed;
    }

    private void OnSpecial()
    {
        if (_isHoldingObject)
        {
            // If the player clicked the mouse wheel and we have a special object grabbed, trigger it
            // Invoke events if of type special object
            GravSpecialObject gsp = (GravSpecialObject)_focusedObject;
            gsp.gravEvents.onGravityGunSpecialTriggered.Invoke();
        }
    }

    /// <summary> Function executed whenever the player is grabbing an object inside Update </summary>
    private void UpdateWhenGrabbingObject()
    {
        // Check if we should change the hold color depending on push range
        if (Vector2.Distance(_gravigunHoldPosDynamic.position, _gravigunHoldPosStatic.position) > _settings.pushRange)
        {
            UpdateLineRenderer(false, Color.clear);
            UpdateTargetCircle(_settings.validTargetLineColor);
            ActivateBezierLines(true);

            currentState = LineState.GrabbingTooFar;
        }
        else
        {
            UpdateLineRenderer(false, Color.clear);
            UpdateTargetCircle(_settings.canPushColor);
            ActivateBezierLines(true);

            currentState = LineState.GrabbingNormal;
        }
    }

    /// <summary> Update loop when not holding an object </summary>
    private void UpdateWhenNotHoldingObject()
    {
        // Wasn't holding, perform as usual
        // Fire a raycast in the direction of the mouse
        RaycastHit2D hit = Physics2D.Raycast(_gravigunPivot.position, _currentLookDir, _settings.maxRaycastDistance, _settings.lineRaycastMask);
        _currentHit = hit;

        if (!hit)
        {
            NoObjectState();
            currentState = LineState.NoObject;
            return;
        }

        // Get game object
        GameObject go1 = hit.collider.gameObject;

        // is it a valid target?
        if (!go1.TryGetComponent(out PhysicsObject physicsObject))
        {
            InvalidObjectState(hit);
            currentState = LineState.InvalidObject;
            return;
        }

        // Keep a reference & update
        _focusedObject = physicsObject;
        _focusedObject.EnableTarget();
        ValidObjectState(hit);

        // Check if ignoring
        if (physicsObject.physicsObjectType == PhysicsObjectType.IgnoresGravigun)
        {
            IgnoreGravigunState();
            currentState = LineState.ObjectIgnoresGravigun;
            return;
        }

        // Check mass limit
        bool isObjectTooHeavy = _focusedObject.rb.mass > _settings.maxMass;
        if (isObjectTooHeavy) { currentState = LineState.TooHeavy; HeavyState(); }
        else { currentState = LineState.ValidObject; }
    }


    private void CheckGrab()
    {
        // Dont attempt to grab object if not of type grabbable or special
        bool isGrabbable = _focusedObject.physicsObjectType is PhysicsObjectType.Grabbable or PhysicsObjectType.Special;
        if (!isGrabbable) return;

        _isNearHoldPos = Vector2.Distance(_focusedObject.transform.position, _gravigunHoldPosDynamic.position) <
                         _settings.grabRange;

        _isInBetweenHoldAndCenter = InBetweenXAxis(_focusedObject.transform.position, _gravigunPivot.position, _gravigunHoldPosDynamic.position);

        // Dont grab object if not in range or if not in between the hold pos and the player
        if (!_isNearHoldPos && !_isInBetweenHoldAndCenter) return;

        // Prevent grabbing if the player is standing on the object
        if (IsPlayerStandingOn()) return;

        // Grab object
        GrabFocusedObject();

        StartCoroutine(LockPullPushRoutine(_settings.pullPushCooldown));
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

    #endregion

    #region Push Logic 

    /// <summary> Performs logic for pushing objects </summary>
    /// <param name="value"></param>
    private void OnPush(InputValue value)
    {
        isPushing = !isPulling && value.isPressed;

        switch (currentState)
        {
            case LineState.NoObject:
                _gravigunNoTarget.PlaySound();
                break;
            case LineState.ValidObject:
                bool inRange = Vector2.Distance(_currentHit.point, _gravigunPivot.position) < _settings.pushRange;
                if (!_isHoldingObject && inRange)
                {
                    // Was within range and can push
                    PerformPush();

                    // Cooldown
                    StartCoroutine(LockPullPushRoutine(_settings.pullPushCooldown));
                }
                else if (inRange) // NOTE: check this
                {
                    _gravigunNoTarget.PlaySound();
                }
                break;
            case LineState.GrabbingTooFar:
                ChangeBezierRendererColor(_settings.validTargetLineColor);
                if (_focusedObject) _focusedObject.ChangeOutlineColor(_settings.validTargetLineColor);
                break;
            case LineState.GrabbingNormal:
                ChangeBezierRendererColor(_settings.canPushColor);
                if (_focusedObject) _focusedObject.ChangeOutlineColor(_settings.canPushColor);

                // Stop holding object
                StopHoldingObject();

                // Perform push
                PerformPush();

                // Trigger cooldown
                StartCoroutine(LockPullPushRoutine(_settings.pullPushCooldown));
                break;
            default:
                break;
        }
    }

    private void CheckPush()
    {
        // Check if in range of pushs
        bool inRange2 = Vector2.Distance(_currentHit.point, _gravigunPivot.position) < _settings.pushRange;
        if (inRange2)
        {
            // Change line renderer color as to show object can be pushed, the same with its outline
            UpdateTargetCircle(_settings.canPushColor);
            UpdateLineRenderer(true, _settings.canPushColor);
            if (_focusedObject) _focusedObject.ChangeOutlineColor(_settings.canPushColor);
        }
        else
        {
            // Default colors
            UpdateLineRenderer(true, _settings.validTargetLineColor);
            if (_focusedObject) _focusedObject.ChangeOutlineColor(_settings.validTargetLineColor);
        }
    }

    /// <summary> Performs a push on the currently focused object </summary>
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

    #endregion

    #region Pull Logic

    /// <summary> Performs logic for pulling objects </summary>
    /// <param name="value"></param>
    private void OnPull(InputValue value)
    {
        isPulling = !isPushing && value.isPressed;
        if (!isPulling && !_isPushPullLocked) _isPlayerHoldingPullAfterGrab = false;

        switch (currentState)
        {
            case LineState.NoObject:
                _gravigunNoTarget.PlaySound();
                break;
            case LineState.TooHeavy:
                _gravigunTooHeavy.PlaySound();
                break;
            case LineState.ValidObject:
                CheckGrab();
                break;
            case LineState.GrabbingTooFar:
            case LineState.GrabbingNormal:
                if (_isPushPullLocked) break;

                // Stop holding object
                StopHoldingObject();

                // Trigger cooldown
                StartCoroutine(LockPullPushRoutine(_settings.pullPushCooldown));
                break;
            default:
                break;
        }

    }

    private void CheckPull()
    {
        // Perform Pull on non grabbed object if set to do so
        if (isPulling && !_isHoldingObject)
        {
            PerformPull();
            // Trigger a longer cooldown routine to avoid prop surfing
            if (IsPlayerStandingOn()) StartCoroutine(LockPullPushRoutine(1f));
        }
        else
        {
            // Refresh the state of the sound bool if not pulling
            _dontPlayPullSound = false;
        }
    }

    /// <summary> Performs a pull on the currently focused object </summary>
    private void PerformPull()
    {
        // Dont perform pull if standing on object
        if (IsPlayerStandingOn()) return;

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


    #endregion

    #region Push/Pull Misc

    /// <summary> Lock the grab for a bit to avoid input overlap </summary>
    private IEnumerator LockPullPushRoutine(float time)
    {
        _isPushPullLocked = true;
        yield return new WaitForSeconds(time);
        _isPushPullLocked = false;
    }




    private void DetermineStates()
    {
        // Calculate mouse point and line
        UpdateAimAndPivot();

        // Dont do anything if turned off
        if (currentState == LineState.Off)
        {
            OffState();
            return;
        }

        _helperTargetCircleSprite.gameObject.SetActive(true);

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




    /// <summary> Handles all physics and input logic. </summary>
    private void HandlePhysicsAndInput()
    {
        // Dont do anything if no focused object available
        if (!_focusedObject) return;

        // check if ignoring
        if (currentState == LineState.ObjectIgnoresGravigun) return;

        // Was an influenceable object, Enable outline and set line renderer
        if (!_trackedObjects.Contains(_focusedObject))
        {
            StartCoroutine(TrackFocusedObjectLeftRoutine(_focusedObject));

            // check if too heavy
            if (currentState == LineState.TooHeavy) return;
        }

        // Move focused object if grabbing
        if (_isHoldingObject)
        {
            // -- Check to see if we should still be holding the object 

            // If the player stands on top of the object, break hold
            if (IsPlayerStandingOn())
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

            // -- Dont do anything else if on cooldown
            if (_isPushPullLocked) return;

            // Start coroutine
            if (_fadeHoldLinesCo == null) _fadeHoldLinesCo = StartCoroutine(FadeHoldLinesRoutine());

            // Break if the object has gone too far from hold position
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


            // Check if in range of pushing
            bool inRange = Vector2.Distance(_focusedObject.transform.position, _gravigunPivot.position) < _settings.pushRange;
            if (inRange) { currentState = LineState.GrabbingNormal; }
            else { currentState = LineState.GrabbingTooFar; }
        }

        CheckPull();
        CheckPush();
    }

    /// <summary> 
    /// Checks if the player is standing on top of the object.
    /// Uses math and not raycasting for better performance. 
    /// </summary>
    private bool IsPlayerStandingOn()
    {
        Vector2 localScale = _focusedObject.transform.localScale;
        Vector2 checkBoxCenter = (Vector2)_focusedObject.transform.position + Vector2.up * (_standingOnCheckBoxHeight / 2 - localScale.y / 3);
        Vector2 checkBoxSize = new Vector2(localScale.x, _standingOnCheckBoxHeight);

        Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.y);

        float distanceY = playerPosition.y - checkBoxCenter.y;
        float distanceX = playerPosition.x - checkBoxCenter.x;

        if (distanceY < checkBoxSize.y && distanceY > 0 &&
            distanceX < checkBoxSize.x && distanceX > 0)
        { return true; }

        return false;
    }

    /// <summary> Adds force towards target without going over the velocity cap </summary>
    private void ApplyCappedForce(Rigidbody2D rb, Vector2 dir, float force, ForceMode2D mode, float maxVel)
    {
        dir = dir.normalized;
        Vector2 v = rb.linearVelocity;
        float along = Vector2.Dot(v, dir); // signed speed toward dir

        // accelerate only while below the cap
        if (along < maxVel) rb.AddForce(dir * force, mode);
    }

    #endregion

    #region Visual Effects 

    /// <summary>
    /// Helper function to update line renderer and its color. 
    /// Put any color when the renderer is off, it will not update. 
    /// </summary>
    /// <param name="on"></param>
    /// <param name="targetColor"></param>
    private void UpdateLineRenderer(bool on, Color targetColor)
    {
        _lineOfSightRenderer.gameObject.SetActive(on);

        if (on)
        {
            _lineOfSightRenderer.startColor = targetColor;
            _lineOfSightRenderer.endColor = targetColor;
        }
    }

    /// <summary>
    /// Helper function to update line renderer to face a certain direction
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="dir"></param>
    /// <param name="target"></param>
    /// <param name="distance"></param>
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

    /// <summary> Helper function to update target circle color and position </summary>
    /// <param name="color"></param>
    private void UpdateTargetCircle(Color color)
    {
        _helperTargetCircleSprite.color = color;
        if (_focusedObject) { _helperTargetCircleSprite.transform.position = _focusedObject.transform.position; }
    }

    /// <summary> Helper method that will change the color of the bezier hold lines </summary>
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

    private void ActivateBezierLines(bool on)
    {
        _bezierLineOneRenderer.enabled = on;
        _bezierLineTwoRenderer.enabled = on;
        _holdLineRenderer.enabled = on;
    }

    /// <summary> Coroutine that handles the fading of the lines in case an 
    /// object blocks the path of the held object and the player </summary>
    private IEnumerator FadeHoldLinesRoutine()
    {
        float time = 0f;

        // Loop
        while (_isHoldingObject)
        {
            // Timer
            time += Time.deltaTime;

            // Check if we have passed the break limit
            if (time >= _settings.blockingObjectTimeBreak)
            {
                // we did pass the limit, break hold
                StopHoldingObject();
                break;
            }

            // Math
            float a = Mathf.Lerp(1f, 0f, time / _settings.blockingObjectTimeBreak);

            // Perform a raycast
            float distance = Vector2.Distance(_gravigunPivot.position, _focusedObject.transform.position);
            Vector2 dir = (_focusedObject.transform.position - _gravigunPivot.position).normalized;
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
            time = 0;
            SetAlphaAllColors(1f);
            yield return null;
        }

        // Not holding an object anymore
        SetAlphaAllColors(1f);
        _fadeHoldLinesCo = null;
    }

    /// <summary> Function to help the FadeHoldLinesRoutine as to fade lines as the sight breaks </summary>
    /// <param name="alpha"> Alpha value between 1 and 0 </param>
    private void SetAlphaAllColors(float alpha)
    {
        _settings.defaultLineOfSightColor.a = alpha;
        _settings.validTargetLineColor.a = alpha;
        _settings.canPushColor.a = alpha;
    }
    /// <summary> Function used to avoid visual bugs when triggering the hold lines </summary>
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


    #endregion
}
