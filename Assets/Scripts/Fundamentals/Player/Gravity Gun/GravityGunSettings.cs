using System.Collections;
using System.Collections.Generic;
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
 * Pupose:
 * 
 */// --------------------------------------------------------


/// <summary>
/// 
/// </summary>
[CreateAssetMenu(fileName = "GravityGunSettings", menuName = "GravityGun/GravityGunSettings")]
public class GravityGunSettings : ScriptableObject
{
    [Header("Limiters")]
    [Range(0f, 100f)] public float maxMass; // The max amount of mass the gravigun can influence
    [Range(0f, 100f)] public float maxVelocity; // Cap the velocity at which we pull an object
    [Range(0f, 100f)] public float pushRange; // Range in which the push works
    [Range(0f, 25f)] public float focusedMoveMaxSpeed = 25f;  // safety cap
    [Range(0f, 10f)] public float grabRange; // Range where the pull grabs the object
    [Range(0f, 50f)] public float WheelMoveMaxDistance; // Max distance at which the player can move an object
    [Range(0f, 100f)] public float grabDistanceBreak; // The distance where the grab breaks from the object
    [Range(0f, 2f)] public float pullPushCooldown; // Small input cooldown so player doesnt immediatly drop the object after grabbing it
    
    [Header("Settings")]
    public LayerMask lineRaycastMask; // The mask of all valid gravity gun targets
    [Range(0f,100f)] public float maxRaycastDistance; // Max distance at which perform the raycast for a valid target
    [Range(0f, 100f)] public float pullForce; // Force at which to pull the object
    [Range(0f, 100f)] public float pushForce; // Force at which to launch the object
    [Range(0f, 720f)] public float rotateSpeed; // Speed at which the mousewheel rotates the object
    [Range(0f, 50f)] public float WheelMoveSpeed; // Speed at which the object moves back and forth on mousewheel move
    [Range(0f,50f)] public float focusedMoveBaseSpeed = 6f;   // baseline units/sec at 1‑unit distance
    [Range(0f,5f)] public float focusedMoveStrengthExponent = 1.5f; // curve exponent (1 = linear)
    [Range(0f, 100f)] public float blockingObjectTimeBreak; // Time it will take for the gravity gun to break hold if some object is in the middle of the path
    
    [Header("Colors")]
    public Color defaultLineOfSightColor; // The color of the line when not pointing towards something influenceable
    public Color validTargetLineColor; // The color of the line when its pointing to a valid target
    public Color canPushColor; // The color the bezier lines will turn into if the object can be launched
    public Color tooHeavyColor;
}
