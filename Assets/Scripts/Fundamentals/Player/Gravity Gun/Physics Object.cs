using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Purpose:
 * Provide a class for all physics objects on the game
 */// --------------------------------------------------------


/// <summary>
/// Abstract class that governs all physics objects
/// </summary>
public abstract class PhysicsObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Outline2D _outline;
    public Collider2D collider;
    public Rigidbody2D rb;
    
    [Header("Settings")]
    public PhysicsObjectType physicsObjectType;
    
    [FormerlySerializedAs("isTargeted")]
    [Header("Readouts")]
    [SerializeField, InspectorReadOnly] private bool _isTargeted;
    [SerializeField, InspectorReadOnly] private Vector2 _currVelocityVector;
    [SerializeField, InspectorReadOnly] private float _currVelocityMagnitude;

    private void FixedUpdate()
    {
        _currVelocityVector = rb.linearVelocity;
        _currVelocityMagnitude = rb.linearVelocity.magnitude;
    }
    
    public virtual void ChangeOutlineColor(Color color)
    {
        _outline.ChangeColor(color);
    }

    public virtual void EnableTarget()
    {
        _isTargeted = true;
        _outline.SetOutline(true);
    }

    public virtual void DisableTarget()
    {
        _isTargeted = false;
        _outline.SetOutline(false);
    }
}

public enum PhysicsObjectType
{
    Grabbable,
    Influenceable,
    IgnoresGravigun,
}
