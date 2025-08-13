using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Provide a parent class to all objects that have a special interaction with
/// the gravity gun
/// </summary>
public abstract class GravSpecialObject : PhysicsObject
{
    // Foldout class
    public GravEvents gravEvents;
    
    [System.Serializable]
    public class GravEvents
    {
        // Events to be used by the special objects
        public UnityEvent OnGravityGunGrab;
        public UnityEvent OnGravityGunPull;
        public UnityEvent OnGravityGunLaunch;
        public UnityEvent OnGravityGunDrop;

        /// <summary>
        /// Special Event triggered by the user. For example, firing a held gun
        /// </summary>
        public UnityEvent onGravityGunSpecialTriggered;
    }
}
