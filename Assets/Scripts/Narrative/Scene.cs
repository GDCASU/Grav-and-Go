using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class Scene : MonoBehaviour
{
    /* --------------------------------------------------------
     * Author:
     * Cami Lee
     * 
     * Modified By:
     * 
     * Purpose: Create scenes that read in narrative text & spit out
     * an editable scene.
     * --------------------------------------------------------
    */

    [Header("Narrative Input")]
    public TextAsset script;
    public Sprite textBackground;
    public Sprite sceneBackground;
    public Dialogue dialogue;

    [Header("Scene Running")]
    public bool remakeScript;
    Dialogue.Block currentBlock;
    int dialogueIndex = 0;

    public enum Character { None, Player, NPC }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (remakeScript) { ReadFile(); }
    }

    /// <summary>
    /// Updates the dialogue based on where in the scene we are.
    /// </summary>
    /// <param name="dialogueText"> The text field to be updated. </param>
    /// <returns></returns>
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
        Character currentSpeaker = Character.None;

        foreach (string line in lines)
        {
            if (line.TrimStart().StartsWith('#') || string.IsNullOrWhiteSpace(line)) { continue; } // if is a comment or blank

            else if (IsCharacterName(line) != Character.None) { currentSpeaker = IsCharacterName(line); } // If is a name

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

    /// <summary> 
    /// Method to figure out what style the text should be based on the 
    /// character name. 
    /// </summary>
    private Character IsCharacterName(string text)
    {
        switch (text.Replace(" ", ""))
        {
            case "NPC": return Character.NPC;
            case "Player": return Character.Player;
            default: return Character.None;
        }
    }
}
