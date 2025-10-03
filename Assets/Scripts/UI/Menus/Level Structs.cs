using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * Cami Lee
 * 
 */// --------------------------------------------------------


[System.Serializable]
// Helper structs to store level data in the Level Manager
public struct Level
{
    [StringInList(typeof(PropertyDrawersHelper), "AllSceneNames")] public string name;
    private GameObject player;
    private GameObject[] hazards;
    private GameObject[] objects;

    /// <summary> Finds the active player object in the scene. </summary>
    /// <returns></returns>
    public GameObject GetPlayerObject()
    {
        if (player.IsUnityNull()) player = GameObject.Find("Player");
        return player;
    }

    /// <summary> Finds the hazard objects in the scene. </summary>
    /// <returns></returns>
    public GameObject[] GetHazardObjects()
    {
        if (hazards.IsUnityNull() || hazards.Length <= 0) hazards = GameObject.FindGameObjectsWithTag("Hazard");
        return hazards;
    }

    /// <summary> Finds the physics objects in the scene. </summary>
    /// <returns></returns>
    public GameObject[] GetMoveableObjects()
    {
        if (objects.IsUnityNull() || objects.Length <= 0) objects = GameObject.FindGameObjectsWithTag("Physics Object");
        return objects;
    }

    // CompareTo for sorting
    public int CompareTo(Level other)
    {
        return string.Compare(this.name, other.name, StringComparison.Ordinal);
    }

    // Equals for == and collections
    public bool Equals(Level other)
    {
        return string.Equals(this.name, other.name, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is Level other && Equals(other);
    }

    public override int GetHashCode()
    {
        return name != null ? name.GetHashCode() : 0;
    }

    public static bool operator ==(Level left, Level right) => left.Equals(right);
    public static bool operator !=(Level left, Level right) => !left.Equals(right);
}

[System.Serializable]
public struct LevelStatus
{
    public bool isUnlocked;
    public float bestTime;
}

