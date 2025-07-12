using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;

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
    [SerializeField] private float _gravigunAngleOffset;
    [SerializeField] private Color _defaultLineOfSightColor; // The color of the line when not pointing towards something influenceable
    [SerializeField] private Color _validTargetLineColor; // The color of the line when its pointing to a valid target
    [SerializeField, Range(0f,100f)] private float _maxRaycastDistance;
    [SerializeField] private LayerMask _lineRaycastMask;
    [SerializeField, Range(0f, 100f)] private float _pullForce;
    [SerializeField, Range(0f, 100f)] private float _maxVelocity;
    [SerializeField, Range(0f, 100f)] private float _pushForce;
    [SerializeField, Range(0f, 100f)] private float _pushRange;
    
    [Header("Debugging")]
    [SerializeField, InspectorReadOnly] private PhysicsObject _focusedObject;
    [SerializeField, InspectorReadOnly] private bool _isHoldingObject;
    [SerializeField] private bool doDebugLog;
    
    // Local variables
    private Vector2 _currentLookDir;
    private RaycastHit2D _currentHit;

    private void Update()
    {
        // 1. Convert mouse position to world space
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;          // flatten to sprite’s plane

        // Direction from sprite to mouse
        _currentLookDir = (mouseWorld - _gravigunPivot.position).normalized;

        // Compute angle (0° = +X). Convert to degrees.
        float angle = Mathf.Atan2(_currentLookDir.y, _currentLookDir.x) * Mathf.Rad2Deg;
        
        // 4. Rotate around Z so +Y faces the cursor
        _gravigunPivot.rotation = Quaternion.AngleAxis(angle + _gravigunAngleOffset, Vector3.forward);
        
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
        // Dont do anything if no focused object available
        if (!_focusedObject) return;
        
        // check if ignoring
        if (_focusedObject.physicsObjectType == PhysicsObjectType.IgnoresGravigun) return;
        
        // Was an infuelceable object, Enable outline and set line renderer
        // TODO: Would setting the shader property every fixed update affect performance too much?
        _focusedObject.EnableTarget();
        _focusedObject.ChangeOutlineColor(_validTargetLineColor);
        ChangeTargetCircleColor(_validTargetLineColor);
        ChangeLineRendererColor(_validTargetLineColor);
        
        // Perform Pull on non grabbed object if set to do so
        if (InputManager.Instance.pullHeldDownInput && !_isHoldingObject)
        {
            PerformPull();
            // Null any push input while pulling as to not queue it
            InputManager.Instance.pushInputRecieved = false;
            return;
        }
        
        // Perform push on non grabbed object if set to do so, but ignore if pull is being held down
        if (!InputManager.Instance.pullHeldDownInput && !_isHoldingObject && InputManager.Instance.pushInputRecieved)
        {
            // Null the input field
            InputManager.Instance.pushInputRecieved = false;
            
            // Check if in range
            if (Vector2.Distance(_currentHit.point, _gravigunPivot.position) > _pushRange) return; // not in range
            
            // Was within range
            PerformPush();
        }
        
        // Dont attempt to grab object if not of type grabbable
        if (_focusedObject.physicsObjectType != PhysicsObjectType.Grabbable) return;

        // TODO: GRAVIGUN LOGIC FOR GRABBING
    }

    private void PerformPull()
    {
        Vector2 dir = (_gravigunHoldPos.position - _focusedObject.transform.position);
        ApplyCappedForce(_focusedObject.rb,
            dir,
            _pullForce,
            ForceMode2D.Force,
            _maxVelocity);
    }

    private void PerformPush()
    {
        ApplyCappedForce(_focusedObject.rb,
            _currentLookDir,
            _pushForce,
            ForceMode2D.Impulse,
            _maxVelocity);
    }
    
    /// <summary>
    /// Adds force towards target without going over the velocity cap
    /// </summary>
    private void ApplyCappedForce(Rigidbody2D rb, Vector2 dir, float force, ForceMode2D mode, float maxVel)
    {
        dir = dir.normalized;
        Vector2 v = rb.linearVelocity;               // or rb.linearVelocity if using DOTS
        float along = Vector2.Dot(v, dir);       // signed speed toward dir

        // accelerate while below the cap
        if (along < maxVel) rb.AddForce(dir * force, mode);
        
        // clamp if we overshoot
        if (along > maxVel)
        {
            Vector2 sideways = v - dir * along;     // flush along-component
            rb.linearVelocity = sideways + dir * maxVel;
        }
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
    
    // 
    
    
    #if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        
    }
    
    
    #endif
    
}
