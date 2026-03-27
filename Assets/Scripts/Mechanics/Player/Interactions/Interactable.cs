using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 *
 * Modified By:
 *
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Purpose:
 * Create an interface all scripts can use for linking with the
 * interaction system (Lore Notes, Switches, Etc.)
 * The item must have a non-trigger collider with a rigidbody
 * and have the interactable script be on the base object.
 * I suggest making the rigidbody kinematic if the object doesnt move
 */// --------------------------------------------------------


/// <summary>
/// Abstract Class that defines functions for interactions
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    // variable
    public InteractableEvents interactableEvents;
    
    // Foldout Class
    [System.Serializable]
    public class InteractableEvents
    {
        /// <summary>
        /// <para>Executes Once</para>
        /// Will trigger only once when the player enters the range of the interactable
        /// </summary>
        public UnityEvent OnInteractionEnter;

        /// <summary>
        /// <para>Executes every frame if on range</para>
        /// Will run if the player interaction radius is in range
        /// </summary>
        public UnityEvent OnInteractionStay;

        /// <summary>
        /// <para>Executes Once</para>
        /// Will execute if the object is the closest to the player
        /// </summary>
        public UnityEvent OnFocusEnter;

        /// <summary>
        /// <para>Executes every frame if on range</para>
        /// Will run if the object is the closest to the player on the frame
        /// </summary>
        public UnityEvent OnFocusStay;

        /// <summary>
        /// <para>Executes Once</para>
        /// Will execute when the object no longer is the closest to the player
        /// </summary>
        public UnityEvent OnFocusExit;

        /// <summary>
        /// <para>Executes Per Input</para>
        /// Once the player hits the interaction key, this will execute
        /// </summary>
        public UnityEvent OnInteractionExecuted;

        /// <summary>
        /// <para>Executes Once</para>
        /// Will execute when the player interaction radious no longer reaches the object
        /// </summary>
        public UnityEvent OnInteractionExit;
    }
}
