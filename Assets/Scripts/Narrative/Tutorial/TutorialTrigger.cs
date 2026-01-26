using UnityEngine;

/// <summary>
/// Class that handles tutorial triggers and displays tutorial text.
/// </summary>
public class TutorialTrigger : MonoBehaviour
{
    public string tutorialText; // Text to display in the tutorial
    private bool isTriggered; // Flag to check if the tutorial has been triggered

    /// <summary>
    /// Sets the trigger method to call when the tutorial should be disabled.
    /// </summary>
    public void SetTrigger(System.Action triggerMethod)
    {
        if (!isTriggered)
        {
            triggerMethod.Invoke(); // Call the trigger method
            DisableTutorial(); // Disable the tutorial after triggering
        }
    }

    /// <summary>
    /// Disables the tutorial.
    /// </summary>
    private void DisableTutorial()
    {
        isTriggered = true; // Set the flag to true
        // Additional logic to hide or disable the tutorial UI can be added here
    }
}