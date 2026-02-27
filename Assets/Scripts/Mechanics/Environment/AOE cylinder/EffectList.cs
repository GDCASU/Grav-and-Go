using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Davyd Yehudin
 * 
 * Modified By: Justin Miller
 * Joshua Wright
 * 
 */// --------------------------------------------------------

/// <summary>
/// an effect list for the AOE_cylinder
/// </summary>
public class EffectList
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

    //effect0 (upward force)
    // effect0 (Upward force)
    public static void effect0(GameObject a, float thrust, float resistance)
    {
        if (!a.TryGetComponent<Rigidbody2D>(out Rigidbody2D forced))
        {
            return;
        }

        // Apply force: direction * base thrust * multiplier
        Vector2 force = new Vector2(0, thrust * resistance);
        forced.AddForce(force);
    }

    // effect1 (Right force)
    public static void effect1(GameObject a, float thrust, float resistance)
    {
        if (!a.TryGetComponent<Rigidbody2D>(out Rigidbody2D forced))
        {
            return;
        }

        Vector2 force = new Vector2(thrust * resistance, 0);
        forced.AddForce(force);
    }

    // effect2 (Downward force)
    public static void effect2(GameObject a, float thrust, float resistance)
    {
        if (!a.TryGetComponent<Rigidbody2D>(out Rigidbody2D forced))
        {
            return;
        }

        Vector2 force = new Vector2(0, -thrust * resistance);
        forced.AddForce(force);
    }

    // effect3 (Left force)
    public static void effect3(GameObject a, float thrust, float resistance)
    {
        if (!a.TryGetComponent<Rigidbody2D>(out Rigidbody2D forced))
        {
            return;
        }

        Vector2 force = new Vector2(-thrust * resistance, 0);
        forced.AddForce(force);
    }
    
    //Gravity Well (Brings to Center)
    public static void GravityWell(GameObject pulled, Collider2D towards, float thrust, float resistance)
    {
        Rigidbody2D rb;
        if (!pulled.TryGetComponent<Rigidbody2D>(out rb))
        {
            Debug.Log("(((((");
            return;
        }
        Vector2 force;

        // Calculate the direction vector from the current object to the target
        Vector2 direction = (Vector2)towards.bounds.center - (Vector2)pulled.transform.position;

        // Normalize the vector to get a consistent magnitude (length of 1)
        // This ensures the force applied is the same regardless of distance
        direction.Normalize();

        // Apply force in that direction, multiplied by the thrust value
        force = direction * thrust * resistance;
        rb.AddForce(force);
        Debug.Log(pulled.name);
    }

    //effects 0 and 1 are examples, change if necessary
}
//enum of all effects, don't forget to add the new effect here and then add it to the switch statement in AOE_cylinder
public enum effectsEnum
{
    effect0,
    effect1,
    effect2,
    effect3,
    GravityWell
}
