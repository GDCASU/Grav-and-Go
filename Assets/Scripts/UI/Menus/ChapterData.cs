using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ChapterData", menuName = "Scriptable Objects/ChapterData")]
public class ChapterData : ScriptableObject
{
    [Header("Index = chapter, val = #levels")]
    public int[] TotalLevelsInChapter; // Can be set from editor, index = chapter, value equal the # of levels
    //public Sprite[] LevelPreviews;

    // the indexes ued between all levelselectbutton scripts for keeping track of what is currently selected
    public int selectedLevelIndex; // 
    public int selectedChapterIndex;


    //The two cabinet objects that the script will use for the animations, set from editor
    //public  GameObject cabinetObject1;
    // public  GameObject cabinetObject2;

    private void Start()
    {
        //Index starts at 1 by default
        selectedLevelIndex = 1;
        selectedChapterIndex = 1;
    }
}
