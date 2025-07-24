using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Class that handles a projectile from a gun
/// </summary>
public class GunProjectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D _rb;
    
    [Header("Settings")]
    [SerializeField] private float _projectileSpeed = 3f;
    [SerializeField] private int _damageAmount = 1;
    
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

    private void FixedUpdate()
    {
        // Direction
        Vector2 forward = transform.up;

        // distance to travel this physics tick
        Vector2 step = _projectileSpeed * Time.fixedDeltaTime * forward;

        // move the rigidbody
        _rb.MovePosition(_rb.position + step);
    }

    /// <summary>
    /// Everything that should happen when the projectile hits something
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Attempt to damage target
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(_damageAmount);
        }
        
        // Destroy the bullet
        Destroy(gameObject);
    }
}
