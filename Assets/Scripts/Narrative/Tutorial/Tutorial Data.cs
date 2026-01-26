using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// ScriptableObject that holds tutorial configuration data.
/// Create instances of this and attach them to EnableTutorial.
/// </summary>
[CreateAssetMenu(fileName = "New Tutorial Data", menuName = "Tutorials/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [Header("Tutorial Info")]
    [SerializeField] public string tutorialName; // Name of the tutorial (e.g., "Lever Switch Tutorial")
    
    [Header("Tutorial Content")]
    [SerializeField, TextArea(2, 4)] public string tutorialText = "You can press [input] to switch the lever."; // Text with [input] placeholder
    
    [Header("Input")]
    [SerializeField] public InputAction inputAction; // Input action to display (e.g., Interact)
    
    [Header("Callback")]
    [SerializeField] public UnityEvent callbackEvent; // Event that triggers tutorial completion
}