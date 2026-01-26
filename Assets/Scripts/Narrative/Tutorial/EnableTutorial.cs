using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EnableTutorial : MonoBehaviour
{

    /* -----------------------------------------------------------
     * Author:
     * Joshua Wright
     * 
     * Modified By:
     * 
     * Purpose: Flag an Object as a Tutorial and manage the tutorial state.Tutorials listen to external events and complete when they fire.
     * 
       --------------------------------------------------------
    */

    [Header("Tutorial Data")]
    [SerializeField] TutorialData tutorialData; // ScriptableObject containing all tutorial data

    [Header("Events")]
    [SerializeField] UnityEvent onTutorialCompleted = new UnityEvent();

    // Flag to check if the tutorial has been triggered
    private bool isTriggered;
    // Text component to display tutorial text
    private Text tutorialTextComponent;
    // Reference to the instantiated canvas prefab
    private GameObject canvasInstance;
    // Reference to the Interactable component (if attached)
    private Interactable interactable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Check if tutorial data is assigned
        if (tutorialData == null)
        {
            Debug.LogError("EnableTutorial on " + gameObject.name + " has no TutorialData assigned!");
            return;
        }

        // Auto-detect Interactable component
        interactable = GetComponent<Interactable>();

        if (interactable != null)
        {
            // Subscribe to focus events
            interactable.events.OnFocusEnter.AddListener(OnFocusEnter);
            interactable.events.OnFocusExit.AddListener(OnFocusExit);
            
            // Subscribe to the callback event from tutorial data
            if (tutorialData.callbackEvent != null)
            {
                tutorialData.callbackEvent.AddListener(OnExternalEventTriggered);
            }

            // Don't show canvas until focused
            CreateTutorialCanvas();
            if (canvasInstance != null)
            {
                canvasInstance.SetActive(false);
            }
        }
        else
        {
            // No interactable component, just create the tutorial
            CreateTutorialCanvas();
            
            // Subscribe to the callback event from tutorial data
            if (tutorialData.callbackEvent != null)
            {
                tutorialData.callbackEvent.AddListener(OnExternalEventTriggered);
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from interactable events
        if (interactable != null)
        {
            interactable.events.OnFocusEnter.RemoveListener(OnFocusEnter);
            interactable.events.OnFocusExit.RemoveListener(OnFocusExit);
        }
        
        // Unsubscribe from callback event
        if (tutorialData != null && tutorialData.callbackEvent != null)
        {
            tutorialData.callbackEvent.RemoveListener(OnExternalEventTriggered);
        }
    }

    private void OnFocusEnter()
    {
        if (canvasInstance != null && !isTriggered)
        {
            canvasInstance.SetActive(true);
        }
    }

    private void OnFocusExit()
    {
        if (canvasInstance != null && !isTriggered)
        {
            canvasInstance.SetActive(false);
        }
    }

    /// <summary>
    /// Creates the tutorial canvas by instantiating the prefab.
    /// </summary>
    private void CreateTutorialCanvas()
    {
        if (tutorialData == null)
            return;

        GameObject tutorialCanvasPrefab = Resources.Load<GameObject>("LevelSelectScreen/Tutorials/TutorialCanvas");

        if (tutorialCanvasPrefab == null)
        {
            Debug.LogError("Tutorial Canvas Prefab could not be found at Resources/LevelSelectScreen/Tutorials/TutorialCanvas.prefab!");
            return;
        }

        // Instantiate the prefab at the tutorial object's position
        canvasInstance = Instantiate(tutorialCanvasPrefab, transform.position, Quaternion.identity);

        // Find and update the Text component
        tutorialTextComponent = canvasInstance.GetComponentInChildren<Text>();
        if (tutorialTextComponent != null)
        {
            // Get the key display from the input action
            string keyDisplay = GetInputKeyDisplay();
            
            // Replace [input] placeholder with the actual key
            string finalText = tutorialData.tutorialText.Replace("[input]", keyDisplay);
            
            tutorialTextComponent.text = finalText;
        }
    }

    /// <summary>
    /// Gets the display string for the input action key binding.
    /// </summary>
    private string GetInputKeyDisplay()
    {
        if (tutorialData == null || tutorialData.inputAction == null)
        {
            Debug.LogWarning("InputAction not assigned in TutorialData!");
            return "[Input Not Set]";
        }

        // Get the display string of the first binding
        if (tutorialData.inputAction.bindings.Count > 0)
        {
            return tutorialData.inputAction.bindings[0].ToDisplayString();
        }

        return "[No Binding]";
    }

    /// <summary>
    /// Called when the callback event fires.
    /// </summary>
    private void OnExternalEventTriggered()
    {
        if (!isTriggered)
        {
            CompleteTutorial();
        }
    }

    /// <summary>
    /// Complete the tutorial and disable the canvas.
    /// </summary>
    private void CompleteTutorial()
    {
        if (isTriggered) return;

        isTriggered = true;

        // Disable the canvas instance
        if (canvasInstance != null)
        {
            canvasInstance.SetActive(false);
        }

        // Fire completion event
        onTutorialCompleted.Invoke();
    }
}