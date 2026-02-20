using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using static Dialogue;

public class DialogueManager : MonoBehaviour
{
    public TextBubble textBubblePrefab;

    private bool isRunning;
    private List<Line> currentDialogue;
    private int currentLineIndex;
    private Line currentLine;

    private readonly Dictionary<string, Speaker> currentSpeakers = new();

    public void StartDialogue(Dialogue dialogue)
    {
        isRunning = true;
        currentDialogue = dialogue.InterpretTextAsset();
        currentLineIndex = 0;

        currentSpeakers.Clear();

        // Gets a Dictonary of all speakers before dialog starts
        Speaker[] speakers = FindObjectsByType<Speaker>(FindObjectsSortMode.None);
        foreach(Speaker speaker in speakers)
            currentSpeakers[speaker.speakerID] = speaker;

        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        while (currentLineIndex < currentDialogue.Count)
        {
            currentLine = currentDialogue[currentLineIndex];

            currentLineIndex++;

            if (!textBubblePrefab.IsFinishedTyping /* && buttonPressed*/)
            {
                //Immediately complete text
            }
            else if (textBubblePrefab.IsFinishedTyping /* && buttonPressed*/)
            {
                //Load new text
            }

            DisplayTextBubble(currentLine);
        }
    }

    public bool DialogueRunning()
    {
        return isRunning;
    }

    public void DisplayTextBubble(Line line)
    {
        if (!currentSpeakers.TryGetValue(line.speakerID, out Speaker speaker))
            throw new KeyNotFoundException($"Dialogue does not contain speaker: {line.speakerID}");

        TextBubble textBubble = Instantiate(textBubblePrefab, speaker.transform);

        textBubble.Init(line);
    }
}
