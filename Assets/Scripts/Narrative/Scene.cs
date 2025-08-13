using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class Scene : MonoBehaviour
{
    [Header("Narrative Input")]
    public TextAsset script;
    public Sprite textBackground;
    public Sprite sceneBackground;
    public Dialogue dialogue;

    [Header("Scene Running")]
    public bool remakeScript;
    Dialogue.Block currentBlock;
    int dialogueIndex = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (remakeScript) { ReadFile(); }
    }

    public bool UpdateDialogue(TypewriterText dialogueText)
    {
        if (dialogueIndex >= dialogue.blocks.Count)
        {
            if (dialogueText.UpdateText(currentBlock.speaker + ": " + currentBlock.line))
            {
                return false;
            }
            else return true;
        }

        currentBlock = dialogue.blocks[dialogueIndex];
        if (dialogueText.UpdateText(currentBlock.speaker + ": " + currentBlock.line)) { dialogueIndex++; }

        return true;
    }

    /// <summary>  Takes information from text files and transfers into something the system can read </summary>
    public void ReadFile()
    {
        // split script based on each line
        string scriptText = script.text;
        string[] lines = Regex.Split(scriptText, "\n|\r|\r\n");

        dialogue.blocks = new List<Dialogue.Block>();
        Dialogue.Character currentSpeaker = Dialogue.Character.None;

        foreach (string line in lines)
        {
            if (line.TrimStart().StartsWith('#') || string.IsNullOrWhiteSpace(line)) { continue; } // if is a comment or blank

            else if (IsCharacterName(line) != Dialogue.Character.None) { currentSpeaker = IsCharacterName(line); } // If is a name

            else if (line == "END") { return; }

            else
            {
                Dialogue.Block newBlock = new();
                newBlock.speaker = currentSpeaker;
                newBlock.line = line;
                dialogue.blocks.Add(newBlock);
            }
        }
    }

    private Dialogue.Character IsCharacterName(string text)
    {
        switch (text.Replace(" ", ""))
        {
            case "Cami": return Dialogue.Character.Cami;
            case "Chandler": return Dialogue.Character.Chandler;
            case "Ian": return Dialogue.Character.Ian;
            default: return Dialogue.Character.None;
        }
    }
}
