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
/// Interface to handle the damage dealt by weapons on the game
/// </summary>
public interface IDamageable
{
    public void TakeDamage(int damage, Rigidbody2D source = null);
}
