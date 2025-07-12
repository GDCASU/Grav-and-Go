using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* -----------------------------------------------------------
 * Author:
 * 
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Purpose:
 * 
 */// --------------------------------------------------------


/// <summary>
/// 
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
    
    // Start is called before the first frame update
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
        OnFocusEnter += () => _outline.SetOutline(true);
        OnFocusExit += () => _outline.SetOutline(false);
        // Handle Interaction
        OnInteractionExecuted += () =>
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
        };
    }

    
}
