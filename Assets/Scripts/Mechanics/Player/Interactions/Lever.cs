using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* -----------------------------------------------------------
 * Author:
 * Ian Flethcer
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Class that handles an interactable lever
/// </summary>
public class Lever : Interactable 
{
    [Header("References")]
    [SerializeField] private Outline2D _outline;
    [SerializeField] private Animator _animator;

    [Header("Settings")] 
    [SerializeField] private bool _startOn;
    
    [Header("Events")]
    [SerializeField] private UnityEvent _OnLeverOn;
    [SerializeField] private UnityEvent _OnLeverOff;
    
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    
    // Local variables
    private static readonly int _isOnID = Animator.StringToHash("isOn");
    
    void Start()
    {
        if (_startOn)
        {
            _animator.SetBool(_isOnID, true);
            _OnLeverOn?.Invoke();
        }
        else
        {
            _animator.SetBool(_isOnID, false);
            _OnLeverOff?.Invoke();
        }
        
        // Subscribe to events
        interactableEvents.OnFocusEnter.AddListener(() => _outline.SetOutline(true));
        interactableEvents.OnFocusExit.AddListener(() => _outline.SetOutline(false));
        // Handle Interaction
        interactableEvents.OnInteractionExecuted.AddListener(() =>
        {
            if (_animator.GetBool(_isOnID))
            {
                // Lever is currently on
                _animator.SetBool(_isOnID, false);
                _OnLeverOff?.Invoke();
            }
            else
            {
                // Lever is currently off
                _animator.SetBool(_isOnID, true);
                _OnLeverOn?.Invoke();
            }
        });
    }

    
}
