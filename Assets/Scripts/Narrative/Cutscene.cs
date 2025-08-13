using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour
{
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

        StartDialogue();
    }


    public void OnChangeDialogue()
    {
        if (sceneTransitioning) return;

        if (scenes[sceneIndex].UpdateDialogue(dialogue))
        {
            // continue
        }
        else
        {
            StartCoroutine(SceneTransition());
        }
    }

    private void EndDialogue()
    {
        // Call event or something? Idk depends on the scenes probably
    }

    private void StartDialogue()
    {
        // Update images
        if (scenes[sceneIndex].sceneBackground) { sceneBackground.sprite = scenes[sceneIndex].sceneBackground; }
        if (scenes[sceneIndex].textBackground) { textBackground.sprite = scenes[sceneIndex].textBackground; }
    }

    IEnumerator SceneTransition()
    {
        sceneTransitioning = true;

        // Update images
        sceneIndex++;

        if (sceneIndex >= scenes.Length)
        {
            EndDialogue();
            yield return null;
        }

        if (scenes[sceneIndex].sceneBackground) { sceneBackground.sprite = scenes[sceneIndex].sceneBackground; }
        if (scenes[sceneIndex].textBackground) { textBackground.sprite = scenes[sceneIndex].textBackground; }

        sceneTransitioning = false;

        yield return null;
    }
}
