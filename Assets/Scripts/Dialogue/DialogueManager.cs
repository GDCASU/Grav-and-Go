using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using static Dialogue;

/// <summary>
/// A class made to handle dialogue bubbles and the flow of dialogue.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public TextBubble textBubblePrefab;

    public UnityEvent onContinue = new();

    public static DialogueManager Instance { get; private set; }

    private Queue<Line> currentDialogue = new();
    private Line currentLine;

    private readonly Dictionary<string, Speaker> currentSpeakers = new();
    private TextBubble currentTextBubble;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
            return; 
        }

        Instance = this;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (currentDialogue.Count > 0)
            return;

        currentDialogue = new(dialogue.InterpretTextAsset());

        currentSpeakers.Clear();

        // Gets a Dictonary of all speakers before dialog starts
        Speaker[] speakers = FindObjectsByType<Speaker>(FindObjectsSortMode.None);
        foreach(Speaker speaker in speakers)
            currentSpeakers[speaker.speakerID] = speaker;

        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        if (currentDialogue.Count > 0)
        {
            currentLine = currentDialogue.Dequeue();

            onContinue?.Invoke();

            DisplayTextBubble(currentLine);
        }

        if(currentTextBubble != null)
        {
            onContinue?.Invoke();
        }
    }

    public void DisplayTextBubble(Line line)
    {
        if (!currentSpeakers.TryGetValue(line.speakerID, out Speaker speaker))
            throw new KeyNotFoundException($"Dialogue does not contain speaker: {line.speakerID}");

        currentTextBubble = Instantiate(textBubblePrefab, speaker.transform);
        currentTextBubble.transform.localPosition = speaker.offset;

        currentTextBubble.Init(speaker, line);
    }

    public void OnInteract()
    {
        ContinueDialogue();
    }
}
