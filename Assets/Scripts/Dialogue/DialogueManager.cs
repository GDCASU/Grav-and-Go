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
    private EventReference? eventReference;
    private string currentTextLine;

    public void StartDialogue(Dialogue dialogue)
    {
        isRunning = true;
        currentDialogue = dialogue.InterpretTextAsset();
        currentLineIndex = 0;
    }

    public void ContinueDialogue()
    {
        while (currentLineIndex < currentDialogue.Count)
        {
            eventReference = currentDialogue[currentLineIndex].voiceLine;
            currentTextLine = currentDialogue[currentLineIndex].text;

            currentLineIndex++;

            if (!textBubblePrefab.IsFinishedTyping /* && buttonPressed*/)
            {
                //Immediately complete text
            }
            else if (textBubblePrefab.IsFinishedTyping /* && buttonPressed*/)
            {
                //Load new text
            }

            //Look for available speaker with chosen ID.
            //Give access to where to put bubble and who the speaker is.
            //Display text bubble and start typing.
        }

        textBubblePrefab.Close();
    }

    public bool DialogueRunning()
    {
        return isRunning;
    }
}
