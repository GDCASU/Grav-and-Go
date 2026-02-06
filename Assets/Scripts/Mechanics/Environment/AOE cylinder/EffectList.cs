using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Davyd Yehudin
 * 
 * Modified By: Justin Miller
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
    public static void effect0(GameObject a)
    {
        Rigidbody2D forced;
        if (!a.TryGetComponent<Rigidbody2D>(out forced))
        {
            Debug.Log("(((((");
            return;
        }
        Vector2 force;
        force.x = 0;
        force.y = 100;
        forced.AddForce(force);
        Debug.Log(a.name);
    }
    //effect1 (Right force)
    public static void effect1(GameObject a)
    {
        Rigidbody2D forced;
        if (!a.TryGetComponent<Rigidbody2D>(out forced))
        {
            Debug.Log("(((((");
            return;
        }
        Vector2 force;
        force.y = 0;
        force.x = 100;
        forced.AddForce(force);
        Debug.Log(a.name);
    }

    //effect2 (Downward force)
    public static void effect2(GameObject a)
    {
        Rigidbody2D forced;
        if (!a.TryGetComponent<Rigidbody2D>(out forced))
        {
            Debug.Log("(((((");
            return;
        }
        Vector2 force;
        force.x = 0;
        force.y = -100;
        forced.AddForce(force);
        Debug.Log(a.name);
    }

    //effect3 (Left force)
    public static void effect3(GameObject a)
    {
        Rigidbody2D forced;
        if (!a.TryGetComponent<Rigidbody2D>(out forced))
        {
            Debug.Log("(((((");
            return;
        }
        Vector2 force;
        force.y = 0;
        force.x = -100;
        forced.AddForce(force);
        Debug.Log(a.name);
    }
    
    //effect3 (Left force)
    public static void GravityWell(GameObject a)
    {
        Rigidbody2D forced;
        if (!a.TryGetComponent<Rigidbody2D>(out forced))
        {
            Debug.Log("(((((");
            return;
        }
        Vector2 force;
        force.y = 0;
        force.x = -100;
        forced.AddForce(force);
        Debug.Log(a.name);
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
