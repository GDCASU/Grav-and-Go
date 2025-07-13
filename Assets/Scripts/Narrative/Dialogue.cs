using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    public enum Character { None, Cami, Chandler, Ian, }

    public List<Block> blocks;

    [Serializable]
    public struct Block
    {
        [SerializeField] public Character speaker;
        [SerializeField] public string line;
        [SerializeField] public Color color;
    }
}
