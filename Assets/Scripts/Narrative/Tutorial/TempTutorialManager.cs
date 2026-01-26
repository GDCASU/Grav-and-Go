using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Temporary tutorial manager that automatically creates and sets up EnableTutorial on a Lever.
/// Finds the "Interact" action and connects it to OnInteractionExecuted, then destroys itself.
/// </summary>
public class TempTutorialManager : MonoBehaviour
{
    private void Start()
    {
        // Get or create the EnableTutorial component
        EnableTutorial tutorial = GetComponent<EnableTutorial>();
        if (tutorial == null)
        {
            tutorial = gameObject.AddComponent<EnableTutorial>();
        }

        // Get the Interactable component (Lever inherits from Interactable)
        Interactable interactable = GetComponent<Interactable>();
        if (interactable == null)
        {
            Debug.LogError("TempTutorialManager requires an Interactable component on the same GameObject!");
            Destroy(this);
            return;
        }

        // Find the Interact InputAction from the player's input asset
        InputAction interactAction = FindInteractAction();
        if (interactAction == null)
        {
            Debug.LogError("Could not find 'Interact' action in InputActionAsset!");
            Destroy(this);
            return;
        }

        // Set EnableTutorial fields via reflection
        var inputActionField = typeof(EnableTutorial).GetField("inputAction", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var callbackEventField = typeof(EnableTutorial).GetField("callbackEvent", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var tutorialTextField = typeof(EnableTutorial).GetField("tutorialText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (inputActionField != null)
            inputActionField.SetValue(tutorial, interactAction);

        if (callbackEventField != null)
            callbackEventField.SetValue(tutorial, interactable.events.OnInteractionExecuted);

        if (tutorialTextField != null)
            tutorialTextField.SetValue(tutorial, "You can press [input] to switch the lever.");

        Debug.Log("TempTutorialManager initialized EnableTutorial on " + gameObject.name);

        // Destroy this manager after setup
        Destroy(this);
    }

    /// <summary>
    /// Finds the Interact InputAction by searching for it in all InputActionAssets in the project.
    /// </summary>
    private InputAction FindInteractAction()
    {
        // Try to find InputActionAsset in the scene
        InputActionAsset[] assets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
        
        foreach (var asset in assets)
        {
            InputActionMap actionMap = asset.FindActionMap("Interactions");
            if (actionMap != null)
            {
                InputAction action = actionMap.FindAction("Interact");
                if (action != null)
                    return action;
            }
        }

        return null;
    }
}