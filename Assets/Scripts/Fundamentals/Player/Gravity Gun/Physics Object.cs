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
    
    [Header("Readouts")]
    [SerializeField, InspectorReadOnly] private bool _isTargeted;
    [SerializeField, Vector2Compass, InspectorReadOnly] private Vector2 _currVelocityVector;
    [SerializeField, InspectorReadOnly] private float _currVelocityMagnitude;

    private void FixedUpdate()
    {
        _currVelocityVector = rb.linearVelocity;
        _currVelocityMagnitude = rb.linearVelocity.magnitude;
    }
    
    /// <summary>
    /// Changes the color of the outline, but doesnt enable it.
    /// Mind you, the sprite needs to have padding for the outline to work
    /// </summary>
    /// <param name="color"> The target color to set </param>
    public virtual void ChangeOutlineColor(Color color)
    {
        _outline.ChangeColor(color);
    }
    
    /// <summary>
    /// Enables the outline of the object.
    /// Mind you, the sprite needs to have padding for the outline to work
    /// </summary>
    public virtual void EnableTarget()
    {
        _isTargeted = true;
        _outline.SetOutline(true);
    }
    
    /// <summary>
    /// Disables the outline of the object.
    /// Mind you, the sprite needs to have padding for the outline to work
    /// </summary>
    public virtual void DisableTarget()
    {
        _isTargeted = false;
        _outline.SetOutline(false);
    }
}

/// <summary>
/// Enum that defines physic objects in the game
/// </summary>
public enum PhysicsObjectType
{
    Grabbable,
    Influenceable,
    IgnoresGravigun,
}
