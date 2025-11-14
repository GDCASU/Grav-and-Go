using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * Cami Lee
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Pupose:
 * Classes to store data written to file
 */// --------------------------------------------------------


/// <summary>
/// General game data that is too big for playerprefs --> use sparingly
/// 
/// NOTE: This class has not been tested. We want to avoid serializing data
/// if possible because of how easily it can corrupt the system and how large 
/// it can make game files. 
/// 
/// If we were to implement, we would also want to convert the serialzed data 
/// to binary for extra security (AVOIDING USING BINARY FORMATTER AS IT IS UNSAFE). 
/// </summary>
[System.Serializable]
public class GameData 
{
    // Weapons

    // Abilities

    // Misc
    public bool testBool;

    /// <summary>
    /// Constructor that will be called when creating a new game
    /// </summary>
    public GameData()
    {
        // Bools are false by default, so no need to set them here
    }

}


/// <summary>
/// Data that is only saved for the active level (leaving a level or exiting the game resets it)
/// </summary>
[System.Serializable]
public class LevelData 
{
    // Player
    Vector3 playerPos;

    // Objects
    List<(Vector3 pos, Quaternion rot)> hazards;
    List<(Vector3 pos, Quaternion rot)> physics;

    // Other

    public void Save(Level level)
    {
        GameObject player = level.GetPlayerObject();
        GameObject[] hazardObjs = level.GetHazardObjects();
        GameObject[] physicsObjs = level.GetMoveableObjects();

        playerPos = player.transform.position;
        hazards = new();
        physics = new();

        for (int i = 0; i < hazardObjs.Length; i++) 
        {
            hazards.Add((hazardObjs[i].transform.position, hazardObjs[i].transform.rotation));
        }

        for (int i = 0; i < physicsObjs.Length; i++)
        {
            physics.Add((physicsObjs[i].transform.position, physicsObjs[i].transform.rotation));
        }
    }

    public void Load(Level level)
    {
        if (hazards == null || physics == null)
        {
            Debug.Log($"Level {level.name} not initialized. Saving data instead.");
            Save(level);
            return;
        }

        GameObject player = level.GetPlayerObject();
        GameObject[] hazardObjs = level.GetHazardObjects();
        GameObject[] physicsObjs = level.GetMoveableObjects();

        player.transform.position = playerPos;

        for (int i = 0; i < hazardObjs.Length; i++)
        {
            hazardObjs[i].transform.SetPositionAndRotation(hazards[i].pos, hazards[i].rot);
        }

        for (int i = 0; i < physicsObjs.Length; i++)
        {
            physicsObjs[i].transform.SetPositionAndRotation(physics[i].pos, physics[i].rot);
            Rigidbody2D rgd2d = physicsObjs[i].GetComponent<Rigidbody2D>();

            // set velocity to 0 to avoid weird movement after load
            if (rgd2d != null) { rgd2d.linearVelocity = Vector2.zero; rgd2d.angularVelocity = 0; }
        }
    }
}

/// <summary>
/// small data that can easily be stored as an int, string, or float
/// </summary>
[System.Serializable]
public class ConfigData 
{
    public Dictionary<string, int> intDict;
    public Dictionary<string, string> stringDict;
    public Dictionary<string, float> floatDict;

    /// <summary> Constructor that will be called when creating a new game </summary>
    public ConfigData()
    {
        // -- Add any relevant data below -- // 

        intDict = new() {
            // -- Levels -- //
            { "currentLevel", 1 }, // tells us how many levels are unlocked

            // -- Framerate (0 False, 1 True) -- //
            { "capFrameRate", 0 },
            { "hideCursor", 0 },
            { "lockCursor", 0 },
            { "confineCursor", 0 },
        };

        floatDict = new()
        {
            // -- Volume -- //
            { "masterVolume", 0.5f },
            { "sfxVolume", 0.5f },
            { "musicVolume", 0.5f },
        };
    }

    public void Save()
    {
        foreach ((string key, int value) in intDict)
        {
            PlayerPrefs.SetInt(key, value);
        }

        foreach ((string key, string value) in stringDict)
        {
            PlayerPrefs.SetString(key, value);
        }

        foreach ((string key, float value) in floatDict)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        PlayerPrefs.Save();
    }

    public void Load()
    {
        foreach ((string key, int defaultValue) in intDict)
        {
            intDict[key] = PlayerPrefs.GetInt(key, defaultValue);
        }

        foreach ((string key, string defaultValue) in stringDict)
        {
            stringDict[key] = PlayerPrefs.GetString(key, defaultValue);
        }

        foreach ((string key, float defaultValue) in floatDict)
        {
            floatDict[key] = PlayerPrefs.GetFloat(key, defaultValue);
        }
    }
}
