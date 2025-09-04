using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Pupose:
 * Handles the connection between the file manager and the
 * game objects
 */// --------------------------------------------------------

/// <summary>
/// Singleton class to be called for saving and loading
/// </summary>
public class SerializedDataManager : MonoBehaviour
{
    // Singleton
    public static SerializedDataManager Instance { get; private set;}
    
    // Data structures
    public GameData gameData { get; private set; }
    public ConfigData configData { get; private set; }

    // Event that will be raised telling objects to start saving if the application is quit
    public static event System.Action StartSavingEvent;

    // Inspector variables
    [Header("Settings")]
    [SerializeField] private string _saveFileName;
    [SerializeField] private string _configFileName;
    
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    
    [Header("Readouts")]
    [SerializeField, InspectorReadOnly, Tooltip("Change when the game name is decided")] private string _saveFolderName = "VGDC2025-26 SaveData";

    [SerializeField, InspectorReadOnly] private string _defaultSaveFileName = "VGDC Game Save";
    [SerializeField, InspectorReadOnly] private string _defaultConfigFileName = "VGDC Game Config";
    [InspectorReadOnly] public bool hasLoaded;
    [InspectorReadOnly] public int objectsWithDataOpened = 0;
    
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
        _configFileName = _configFileName.Trim();
        
        // Check for empty fields
        if (_saveFileName.Length <= 0)
        {
            Debug.LogWarning("No save file name provided, using default save file name");
            _saveFileName = _defaultSaveFileName;
        }

        if (_configFileName.Length <= 0)
        {
            Debug.LogWarning("No config file name provided, using default config file name");
            _configFileName = _defaultConfigFileName;
        }

        // Load Data
        fileDataHandler = new FileDataHandler(_saveFolderName, _saveFileName, _configFileName);
        LoadGame();
        hasLoaded = true;
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void NewConfigs()
    {
        this.configData = new ConfigData();
    }

    public void LoadGame()
    {
        // Load any saved data from a file using the data handler
        this.gameData = fileDataHandler.LoadGameData();
        this.configData = fileDataHandler.LoadConfigData();
        
        // if no game data is found, create a new game
        if (this.gameData == null)
        {
            if (_doDebugLog) Debug.Log("<color=yellow>No game data found, initializing to default values</color>");
            NewGame();
        }

        // if no config data found, create new ones
        if (this.configData == null)
        {
            if (_doDebugLog) Debug.Log("<color=yellow>No config data found, initializing to default values</color>");
            NewConfigs();
        }

        // Debugging
        if (_doDebugLog) Debug.Log("Loaded data to objects");
    }

    // On destroy is called after on application quit
    public void OnApplicationQuit()
    {
        // Call the saving event on all objects so they save before writting to file
        StartSavingEvent?.Invoke();
        // Save all data to file once the game quit
        SaveData();
    }

    public void SaveData()
    {
        // Save the data to a file using the data handler
        fileDataHandler.SaveGameData(gameData);
        fileDataHandler.SaveConfigData(configData);
    }

    #region Unlocking Functions
    // Interface functions will reside here, stuff like unlock gatling, cheats or scenes


    #endregion
}
