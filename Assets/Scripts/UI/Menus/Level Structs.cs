using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------


[System.Serializable]
// Helper structs to store level data in the Level manager
public struct LevelName
{
    [StringInList(typeof(PropertyDrawersHelper), "AllSceneNames")] public string name;
    
    // CompareTo for sorting
    public int CompareTo(LevelName other)
    {
        return string.Compare(this.name, other.name, StringComparison.Ordinal);
    }

    // Equals for == and collections
    public bool Equals(LevelName other)
    {
        return string.Equals(this.name, other.name, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is LevelName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return name != null ? name.GetHashCode() : 0;
    }

    public static bool operator ==(LevelName left, LevelName right) => left.Equals(right);
    public static bool operator !=(LevelName left, LevelName right) => !left.Equals(right);
}

[System.Serializable]
public struct LevelStatus
{
    public bool isUnlocked;
    public float bestTime;
}

