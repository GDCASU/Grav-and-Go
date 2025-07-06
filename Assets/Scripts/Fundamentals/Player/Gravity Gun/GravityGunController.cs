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
    [SerializeField, Range(0f, 100f)] private float _maxPullVelocity;
    [SerializeField, Range(0f, 100f)] private float _pushForce;
    
    [Header("Debugging")]
    [SerializeField, InspectorReadOnly] private PhysicsObject _focusedObject;
    [SerializeField, InspectorReadOnly] private bool _isHoldingObject;
    [SerializeField] private bool doDebugLog;
    
    // Local variables
    private Vector2 _currentLookDir;

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
        if (!go1.TryGetComponent(out PhysicsObject grabbableObject))
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
        // Enable outline
        grabbableObject.EnableTarget();
        grabbableObject.ChangeOutlineColor(_validTargetLineColor);
        _focusedObject = grabbableObject;

        // Update Line Renderer
        ChangeTargetCircleColor(_validTargetLineColor);
        ChangeLineRendererColor(_validTargetLineColor);
    }

    // Physics computation
    private void FixedUpdate()
    {
        // Dont do anything if no focused object available
        if (!_focusedObject) return;
        
        // check if ignoring
        if (_focusedObject.physicsObjectType == PhysicsObjectType.IgnoresGravigun) return;
        
        // Pull object
        if (InputManager.Instance.pullHeldDownInput && !_isHoldingObject)
        {
            PerformPull();
            return;
        }
        
        
        switch (_focusedObject.physicsObjectType)
        {
            case PhysicsObjectType.Influenceable:
                //_focusedObject.rb.AddForce(pullForce, ForceMode2D.Force);
                break;
            case PhysicsObjectType.Grabbable:
                //
                break;
        }

        // TODO: GRAVIGUN LOGIC AND DIFFERENTIATING FROM INFLUENCEABLE AND GRABBABLE
    }

    private void PerformPull()
    {
        // it was a influenceable object, compute force towards pivot force without going over max velocity
        Vector2 dir = (_gravigunHoldPos.position - _focusedObject.transform.position).normalized;
        Vector2 v = _focusedObject.rb.linearVelocity;
        float along = Vector2.Dot(v, dir);
        // case 1: we’re still under the cap, keep adding force
        if (along < _maxPullVelocity)
        {
            Vector2 pull = dir * _pullForce; // N = kg·m/s²
            _focusedObject.rb.AddForce(pull, ForceMode2D.Force);
        }

        // Case 2: we’ve reached / exceeded the cap → clamp the velocity
        if (along > _maxPullVelocity)
        {
            // keep the sideways component unchanged, but set the along-component
            // to exactly the cap (so we don’t jitter back and forth)
            Vector2 sideways = v - dir * along; // remove along-component
            _focusedObject.rb.linearVelocity = sideways + dir * _maxPullVelocity;
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
