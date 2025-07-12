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

/* -----------------------------------------------------------
 * Purpose:
 * Governs all physics objects that can be grabbed by the
 * gravigun
 */// --------------------------------------------------------


/// <summary>
/// Class that governs all physics objects that can be grabbed
/// </summary>
public class GrabbableObject : PhysicsObject
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
}
