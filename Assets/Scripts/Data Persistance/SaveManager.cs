using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;
using System.IO;
using System;
using Unity.VisualScripting;

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
 * Handles the connection between the file manager and the
 * game objects
 */// --------------------------------------------------------

/// <summary> Singleton class to be called for saving and loading </summary>
public class SaveManager : MonoBehaviour
{
    // Singleton
    public static SaveManager Instance { get; private set; }

    // Data structures
    public GameData gameData { get; private set; }
    public ConfigData configData { get; private set; }
    public LevelData levelData { get; private set; }

    // Event that will be raised telling objects to start saving if the application is quit
    public static event Action StartSavingEvent;

    // Inspector variables
    [Header("Settings")]
    [SerializeField] private string _saveFileName;

    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

    [Header("Readouts")]
    [SerializeField, InspectorReadOnly, Tooltip("Change when the game name is decided")] private string _saveFolderName = "VGDC2025-26 SaveData";

    [SerializeField, InspectorReadOnly] private string _defaultSaveFileName = "VGDC Game Save";
    [InspectorReadOnly] public bool hasLoaded;
    [InspectorReadOnly] public int objectsWithDataOpened = 0;


    public enum DataType { Level, Game, PlayerPref }

    // Local Variables

    private FileDataHandler fileDataHandler;

    private void Awake()
    {
        // Handle Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Clear any leftover spaces
        _saveFileName = _saveFileName.Trim();

        // Check for empty fields
        if (_saveFileName.Length <= 0)
        {
            Debug.LogWarning("No save file name provided, using default save file name");
            _saveFileName = _defaultSaveFileName;
        }

        // Load Data
        fileDataHandler = new FileDataHandler(_saveFolderName);
        LoadGame();
        hasLoaded = true;
    }


    #region InitializeData
    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void NewConfigs()
    {
        this.configData = new ConfigData();
    }
    public void NewLevels()
    {
        this.levelData = new LevelData();
    }
    #endregion


    #region LoadData

    /// <summary> Load data that was serialized. </summary>
    public void LoadGame()
    {
        // Load any saved data from a file using the data handler
        try { this.gameData = fileDataHandler.LoadData<GameData>(_saveFileName, false); }
        catch
        {
            if (_doDebugLog) Debug.Log("<color=yellow>No game data found, initializing to default values</color>");
            NewGame();
        }

        if (_doDebugLog) Debug.Log("Loaded data to objects");
    }

    /// <summary> Update the values in the associated level struct. </summary>
    /// <param name="level"></param>
    public void LoadLevel(Level level)
    {
        if (levelData.IsUnityNull()) NewLevels();
        levelData.Load(level);

        if (_doDebugLog) Debug.Log("Loaded level data");
    }

    /// <summary> Loads int PlayerPref values by their key name </summary>
    /// <param name="key"></param>
    /// <returns> Int value that is in PlayerPrefs</returns>
    public int LoadIntData(string key)
    {
        if (configData.IsUnityNull()) NewConfigs();
        else configData.Load();

        try { configData.intDict.TryGetValue(key, out int value); return value; }
        catch { Debug.LogError($"Key {key} not found."); return -1; }
    }

    /// <summary> Loads float PlayerPref values by their key name </summary>
    /// <param name="key"></param>
    /// <returns> Float value that is in PlayerPrefs</returns>
    public float LoadFloatData(string key)
    {
        if (configData.IsUnityNull()) NewConfigs();
        else configData.Load();

        try { configData.floatDict.TryGetValue(key, out float value); return value; }
        catch { Debug.LogError($"Key {key} not found."); return -1; }
    }

    /// <summary> Loads string PlayerPref values by their key name </summary>
    /// <param name="key"></param>
    /// <returns> String value that is in PlayerPrefs</returns>
    public string LoadStringData(string key)
    {
        if (configData.IsUnityNull()) NewConfigs();
        else configData.Load();

        try { configData.stringDict.TryGetValue(key, out string value); return value; }
        catch { Debug.LogError($"Key {key} not found."); return ""; }
    }

    #endregion


    #region SaveData
    /// <summary> Save data that needs to be serialized. </summary>
    public void SaveGame()
    {
        try { fileDataHandler.SaveData(_saveFileName, gameData, false); }
        catch (Exception e) { Debug.LogError($"Couldn't save data. SaveFile: {_saveFileName}, Error Code: {e}"); }


        if (_doDebugLog) Debug.Log($"Saved serialized game data at {_saveFileName}.");
    }

    /// <summary> Save level data </summary>
    /// <param name="level">The level struct that stores the information that needs to be saved.</param>
    public void SaveLevel(Level level)
    {
        if (levelData.IsUnityNull()) NewLevels();
        levelData.Save(level);

        if (_doDebugLog) Debug.Log($"Saved current level data {level.name}");
    }

    /// <summary>Save PlayerPref values that are ints </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SaveConfigData(string key, int value)
    {
        if (configData.IsUnityNull()) NewConfigs();
        configData.intDict[key] = value;
        configData.Save();
    }

    /// <summary>Save PlayerPref values that are floats </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SaveConfigData(string key, float value)
    {
        if (configData.IsUnityNull()) NewConfigs();
        configData.floatDict[key] = value;
        configData.Save();
    }

    /// <summary>Save PlayerPref values that are strings </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SaveConfigData(string key, string value)
    {
        if (configData.IsUnityNull()) NewConfigs();
        configData.stringDict[key] = value;
        configData.Save();
    }

    #endregion

    #region Unlocking Functions
    // Interface functions will reside here, stuff like unlock gatling, cheats or scenes


    #endregion
}
