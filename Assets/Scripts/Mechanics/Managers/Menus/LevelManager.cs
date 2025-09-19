using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEngine.Events;
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
 * Cody Quinn
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
    [SerializeField] private SerializedDictionary<LevelName, LevelStatus> _levelProgressDatabase;

    [Header("Debugging")] 
    [SerializeField] private bool _doDebugLog;

    [Header("Events")]
	/// Triggered when the player starts playing the level
    public UnityEvent<LevelName> OnLevelStart { get; private set; } = new UnityEvent<LevelName>();

	/// Triggered when the level is completed successfully
    public UnityEvent<LevelName> OnLevelComplete { get; private set; } = new UnityEvent<LevelName>();

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

    /// <summary>
    /// Function that loads a level via LevelName only if unlocked
    /// </summary>
    /// <param name="levelName"> The name of the level </param>
    /// <returns> Returns false if failed, otherwise true for succeeded </returns>
    public bool LoadLevelViaLevelNameIfUnlocked(LevelName levelName)
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
        if (isUnlocked) LoadLevelScene(levelName);
        return isUnlocked;
    }

    /// <summary>
    /// Function that loads a level via LevelName
    /// </summary>
    /// <param name="levelName"> The name of the level, set in the inspector please </param>
    /// <returns> False if failed </returns>
    public bool LoadLevelViaLevelName(LevelName levelName)
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
        LoadLevelScene(levelName);
        return true;
    }

    private void LoadLevelScene(LevelName levelName)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelName.name);

        loadOperation.completed += (_ =>
        {
            OnLevelStart.Invoke(levelName);
        });
    }
    
    /// <summary>
    /// Function that checks if a certain level is unlocked
    /// </summary>
    /// <param name="levelName"> The name of the level, set in the inspector please </param>
    /// <returns> False if not unlocked </returns>
    public bool IsLevelUnlocked(LevelName levelName)
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

    /// <summary>
    /// Function that will get the best time in the level
    /// </summary>
    /// <param name="levelName">The name of the level</param>
    /// <returns>The best time of the level. If the level hasn't been played 0, if there was an error -1.</returns>
    public float GetLevelBestTime(LevelName levelName)
    {
        // Check if the level is in database
        bool status = _levelProgressDatabase.TryGetValue(levelName, out LevelStatus level);
        if (!status)
        {
            Debug.LogWarning($"The level {levelName} is not defined in the level database.");
            return -1;
        }

        return level.bestTime;
    }

    /// <summary>
    /// Function that will update the best time in the level progress database if time is less than the current best
    /// time
    /// </summary>
    /// <param name="levelName">The name of the level</param>
    /// <param name="time">Time in milliseconds</param>
    /// <returns>True if new time is best</returns>
    public bool UpdateLevelBestTime(LevelName levelName, float time)
    {
        // Check if the level is in database
        bool status = _levelProgressDatabase.TryGetValue(levelName, out LevelStatus level);
        if (!status)
        {
            Debug.LogWarning($"The level {levelName} is not defined in the level database.");
            return false;
        }

        // If the new time is less than the current best time or hasn't been played, update the best time
        if (time < level.bestTime || time == 0)
        {
            level.bestTime = time;
            return true;
        }

        return false;
    }
    
}


