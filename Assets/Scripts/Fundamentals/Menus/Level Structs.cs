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
}

[System.Serializable]
public struct UnlockedStatus
{
    public bool isUnlocked;
}

