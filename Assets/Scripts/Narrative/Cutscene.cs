using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour
{
    /* --------------------------------------------------------
     * Author:
     * Cami Lee
     * 
     * Modified By:
     * 
     * Purpose: Manage scenes and the transitions between them.
     * --------------------------------------------------------
    */

    [SerializeField, InspectorReadOnly] Scene[] scenes;
    [SerializeField, InspectorReadOnly] Image sceneBackground;
    [SerializeField, InspectorReadOnly] Image textBackground;
    [SerializeField, InspectorReadOnly] TypewriterText dialogue;

    [SerializeField] GlitchManager glitchManager;
    [SerializeField] float sceneTransitionTime = 1;


    Dialogue.Block currentBlock;
    int sceneIndex = 0;

    bool sceneTransitioning;

    private void Start()
    {
        sceneBackground = GetComponent<Image>();
        textBackground = transform.GetChild(0).GetComponent<Image>();
        dialogue = textBackground.gameObject.GetComponentInChildren<TypewriterText>();
        scenes = GetComponentsInChildren<Scene>();

        StartScene();
    }

    /// <summary> Called by Input System to perform changing the dialogue </summary>
    public void OnChangeDialogue()
    {
        // If we are done with this cutscene
        if (sceneIndex >= scenes.Length) return;

        // If we are still going between scenes
        if (sceneTransitioning) return;

        // If we are done with the current dialogue block
        if (!scenes[sceneIndex].UpdateDialogue(dialogue))
        {
            StartCoroutine(SceneTransition());
        }
    }

    /// <summary> Ends the current cutscene. </summary>
    private void EndDialogue()
    {
        // TBD: event that switches to the next scene

        // Make dialogue empty
        dialogue.ClearText();
    }

    /// <summary> Starts a new scene within the current cutscene. </summary>
    private void StartScene()
    {
        // Update images
        if (scenes[sceneIndex].sceneBackground) { sceneBackground.sprite = scenes[sceneIndex].sceneBackground; }
        if (scenes[sceneIndex].textBackground) { textBackground.sprite = scenes[sceneIndex].textBackground; }
    }

    IEnumerator SceneTransition()
    {
        sceneTransitioning = true;
        sceneIndex++;

        if (sceneIndex >= scenes.Length) { EndDialogue(); }

        else
        {
            glitchManager.StartGlitch();
            dialogue.ClearText();

            yield return new WaitForSeconds(sceneTransitionTime/2);

            StartScene();

            yield return new WaitForSeconds(sceneTransitionTime/2);
            glitchManager.StopGlitch();
        }

        sceneTransitioning = false;
        yield return null;
    }
}
