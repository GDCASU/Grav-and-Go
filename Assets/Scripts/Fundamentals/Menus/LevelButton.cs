using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Class that handles a button in the level select window
/// </summary>
public class LevelButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _levelButton;
    
    [Header("Target Level")]
    [SerializeField] private LevelName _levelReference;
    
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    
    private void Start()
    {
        // Check if the level is unlocked
        bool isUnlocked = LevelManager.Instance.IsLevelUnlocked(_levelReference);
        
        // Set if the button is interactable
        _levelButton.interactable = isUnlocked;
    }
    
    /// <summary>
    /// Function that the UI button uses to load the level
    /// </summary>
    public void LoadLevel()
    {
        LevelManager.Instance.LoadLevelViaLevelNameIfUnlocked(_levelReference);
    }
}
