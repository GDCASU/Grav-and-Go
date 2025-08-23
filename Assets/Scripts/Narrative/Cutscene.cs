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

    private void EndDialogue()
    {
        // Call event or something? Idk depends on the scenes probably

        // Make dialogue empty
        dialogue.ClearText();
    }

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
            StartScene();
            dialogue.ClearText();
        }

        sceneTransitioning = false;
        yield return null;
    }
}
