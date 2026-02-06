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
    [Header("Tutorial Content")]
    [SerializeField, TextArea(2, 4)] string tutorialText = "You can press [input] to switch the lever."; // Text with [input] placeholder
    
    [Header("Input")]
    [SerializeField] InputActionReference inputActionReference; // Reference to the InputAction that triggers the tutorial
    
    [Header("UnityEvent")]
    [SerializeField] public UnityEvent callbackEvent; // Event that triggers tutorial completion

    [Header("Canvas")]
    [SerializeField] GameObject customCanvasPrefab; // Custom canvas prefab (optional)
    [SerializeField] string defaultCanvasResourcePath = "LevelSelectScreen/Tutorials/TutorialCanvas"; // Default resource path for canvas prefab

    [Header("Display Behavior")]
    [SerializeField] bool requireFocusToDisplay = true; // If true, tutorial only shows when focused; if false, always visible

    [Header("Completion Events")]
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
        // Auto-detect Interactable component
        interactable = GetComponent<Interactable>();

        if (interactable != null)
        {
            // Subscribe to focus events
            interactable.events.OnFocusEnter.AddListener(OnFocusEnter);
            interactable.events.OnFocusExit.AddListener(OnFocusExit);
            
            // Subscribe to the callback event
            if (callbackEvent != null)
            {
                callbackEvent.AddListener(OnExternalEventTriggered);
            }

            // Create canvas
            CreateTutorialCanvas();
            if (canvasInstance != null)
            {
                // Only hide canvas if it requires focus to display
                canvasInstance.SetActive(!requireFocusToDisplay);
            }
        }
        else
        {
            // No interactable component available
            if (requireFocusToDisplay)
            {
                Debug.LogWarning("EnableTutorial: requireFocusToDisplay is true but no Interactable component found. Tutorial will not display. Either add an Interactable component or set requireFocusToDisplay to false.");
            }
            
            // Create the tutorial
            CreateTutorialCanvas();
            if (canvasInstance != null)
            {
                // Show canvas if not requiring focus
                canvasInstance.SetActive(!requireFocusToDisplay);
            }
            
            // Subscribe to the callback event
            if (callbackEvent != null)
            {
                callbackEvent.AddListener(OnExternalEventTriggered);
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
        if (callbackEvent != null)
        {
            callbackEvent.RemoveListener(OnExternalEventTriggered);
        }
    }

    private void OnFocusEnter()
    {
        if (canvasInstance != null && !isTriggered && requireFocusToDisplay)
        {
            canvasInstance.SetActive(true);
        }
    }

    private void OnFocusExit()
    {
        if (canvasInstance != null && !isTriggered && requireFocusToDisplay)
        {
            canvasInstance.SetActive(false);
        }
    }

    /// <summary>
    /// Creates the tutorial canvas by instantiating the prefab.
    /// </summary>
    private void CreateTutorialCanvas()
    {
        GameObject tutorialCanvasPrefab = null;

        // Try to load custom prefab if assigned
        if (customCanvasPrefab != null)
        {
            tutorialCanvasPrefab = customCanvasPrefab;
        }
        else
        {
            // Fall back to default prefab path
            tutorialCanvasPrefab = Resources.Load<GameObject>(defaultCanvasResourcePath);
        }

        if (tutorialCanvasPrefab == null)
        {
            Debug.LogError($"Tutorial Canvas Prefab could not be found! Assign a prefab to customCanvasPrefab or ensure Resources/{defaultCanvasResourcePath}.prefab exists!");
            return;
        }

        // Instantiate the prefab at the tutorial object's position
        canvasInstance = Instantiate(tutorialCanvasPrefab, transform.position, Quaternion.identity);

        // Find and update the Text component if it exists
        tutorialTextComponent = canvasInstance.GetComponentInChildren<Text>();
        if (tutorialTextComponent != null)
        {
            // Get the key display from the input action
            string keyDisplay = GetInputKeyDisplay();
            
            // Replace [input] placeholder with the actual key
            string finalText = tutorialText.Replace("[input]", keyDisplay);
            
            tutorialTextComponent.text = finalText;
        }
        else
        {
            Debug.LogWarning("No Text component found in the instantiated canvas. The tutorial text will not be displayed.");
        }
    }

    /// <summary>
    /// Gets the display string for the input action key binding.
    /// </summary>
    private string GetInputKeyDisplay()
    {
        if (inputActionReference == null)
        {
            Debug.LogWarning("InputActionReference not assigned in EnableTutorial!");
            return "[Input Not Set]";
        }

        InputAction inputAction = inputActionReference.action;
        if (inputAction == null)
        {
            Debug.LogWarning("InputAction is null in InputActionReference!");
            return "[Input Not Set]";
        }

        // Get the display string of the first binding
        if (inputAction.bindings.Count > 0)
        {
            return inputAction.bindings[0].ToDisplayString();
        }

        return "[No Binding]";
    }

    /// <summary>
    /// Called when the callback event fires.
    /// </summary>
    public void OnExternalEventTriggered()
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