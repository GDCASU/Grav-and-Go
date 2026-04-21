using UnityEngine;
using FMODUnity;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    public TextAsset textAsset;
    public EventReference[] voiceLines;

    /// <summary>
    /// Parses <b>textAsset</b> and connects it to <b>voiceLines</b> to generate a list of lines
    /// </summary>
    /// <returns>The list of the dialogue's lines. If the line does not have a voice line for it, it will be null</returns>
    /// <exception cref="InvalidDialogueLineException"></exception>
    public List<Line> InterpretTextAsset()
    {
        List<Line> result = new();

        string[] lines = textAsset.text.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);
        
        for(int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(':', 2);

            if (parts.Length != 2) throw new InvalidDialogueLineException($"Invalid dialogue line found on line {i + 1}:\n{lines[i]}");

            EventReference? voiceLine = i < voiceLines.Length ? voiceLines[i] : null;
            string text = parts[1].Trim();
            string speakerID = parts[0].Trim();

            Line line = new(speakerID, text, voiceLine);

            result.Add(line);
        }

        return result;
    }

    [System.Serializable]
    public struct Line
    {
        public string text;
        public string speakerID;
        public EventReference? voiceLine;

        public Line(string speakerID, string text, EventReference? voiceLine)
        {
            this.text = text;
            this.speakerID = speakerID;
            this.voiceLine = voiceLine;
        }
    }

    public class InvalidDialogueLineException : System.Exception
    {
        public InvalidDialogueLineException(string message) => new System.Exception(message);
    }
}
