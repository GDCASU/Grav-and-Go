using UnityEngine;
using FMOD;
using FMODUnity;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    public TextAsset textAsset;
    public EventReference[] voiceLines;

    public List<Line> InterpretTextAsset()
    {
        return null; //Stub
    }

    [System.Serializable]
    public struct Line
    {
        public string text;
        public string speakerID;
        public EventReference? voiceLine;

        public Line(string text, string speakerID, EventReference? voiceLine)
        {
            this.text = text;
            this.speakerID = speakerID;
            this.voiceLine = voiceLine;
        }
    }
}
