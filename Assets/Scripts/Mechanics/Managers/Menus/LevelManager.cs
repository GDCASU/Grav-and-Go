using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 *
 * Modified By:
 * Cami Lee
 * 
 */// --------------------------------------------------------

/// <summary>
/// Class that handles the level data of the game, and starting level changes
/// </summary>
public class LevelManager : MonoBehaviour
{
    // Singleton
    public static LevelManager Instance { get; private set; }

    [FormerlySerializedAs("levelProgressDatabase")]
    [Header("Database")] 
    [SerializeField] private SerializedDictionary<Level, LevelStatus> _levelProgressDatabase;
    Level currentLevelName;
    [SerializeField] Level levelSelect;
    [SerializeField] Level mainMenu;

    [Header("Player")]
    PlayerMovementController playerController;

    [Header("Debugging")] 
    [SerializeField] private bool _doDebugLog;

    void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerMovementController>();
    }

    /// <summary> Function that loads a level via LevelName only if unlocked </summary>
    /// <param name="levelName"> The name of the level </param>
    /// <returns> Returns false if failed, otherwise true for succeeded </returns>
    public bool LoadLevelViaLevelNameIfUnlocked(Level levelName)
    {
        // Check if in dictionary
        bool status = _levelProgressDatabase.TryGetValue(levelName, out LevelStatus unlockedStatus);
        if (!status)
        {
            // Didnt find it in dictionary
            Debug.LogWarning($"The level {levelName} is not defined in the level database.");
            return false;
        }
        
        // Was defined, check if unlocked
        bool isUnlocked = IsLevelUnlocked(levelName);
        if (isUnlocked) SceneManager.LoadSceneAsync(levelName.name);
        return isUnlocked;
    }

    /// <summary> Function that loads a level via LevelName </summary>
    /// <param name="levelName"> The name of the level, set in the inspector please </param>
    /// <returns> False if failed </returns>
    public bool LoadLevelViaLevelName(Level levelName)
    {
        // Check if in dictionary
        bool status = _levelProgressDatabase.TryGetValue(levelName, out LevelStatus unlockedStatus);
        if (!status)
        {
            // Didnt find it in dictionary
            Debug.LogWarning($"The level {levelName} is not defined in the level database.");
            return false;
        }

        // Was defined
        currentLevelName = levelName;
        StartCoroutine(LoadLevel());
        return true;
    }

    public void ReloadCurrentLevel()
    {
        LoadLevelViaLevelName(currentLevelName);
    }
    
    public void LoadLevelSelect()
    {
        LoadLevelViaLevelName(levelSelect);
    }

    public void LoadMainMenu()
    {
        LoadLevelViaLevelName(mainMenu);
    }

    /// <summary> Function that checks if a certain level is unlocked </summary>
    /// <param name="levelName"> The name of the level, set in the inspector please </param>
    /// <returns> False if not unlocked </returns>
    public bool IsLevelUnlocked(Level levelName)
    {
        // Check if level is in database
        bool status = _levelProgressDatabase.TryGetValue(levelName, out LevelStatus value);
        if (!status)
        {
            // Not in database
            Debug.LogWarning($"The level {levelName} is not defined in the level database.");
            return false;
        }
        
        // Return unlocked status
        return value.isUnlocked;
    }

    public void SaveCheckpoint()
    {
        SaveManager.Instance.SaveLevel(currentLevelName);
        if (_doDebugLog) Debug.Log($"Saving level {currentLevelName.name}");
    }

    public void LoadLastCheckpoint()
    {
        DeathManager.ResetDeathState();
        SaveManager.Instance.LoadLevel(currentLevelName);
        playerController.enabled = true;
        if (_doDebugLog) Debug.Log($"Loading level {currentLevelName.name}");
    }

    public void LoadLastCheckpoint(int delay)
    {
        StartCoroutine(LoadCheckpoint(delay));
    }

    IEnumerator LoadCheckpoint(int delay)
    {
        yield return new WaitForSeconds(delay);
        LoadLastCheckpoint();
    }

    IEnumerator LoadLevel()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(currentLevelName.name);

        while (!load.isDone)
        {
            yield return null;
        }

        if (currentLevelName.name != "Main Menu" 
            && currentLevelName.name != "Level Select") SaveManager.Instance.SaveLevel(currentLevelName);
    }
}


