using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 *
 * Modified By:
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Pupose:
 * Set up some global configurations on the game
 */// --------------------------------------------------------

/// <summary>
/// Class that holds the settings of the game
/// </summary>
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;        // Singleton reference

    [Header("General")]
    private SaveManager save;

    [Header("Cursor")]
    [SerializeField] private bool hideCursor;
    [SerializeField] private bool lockCursor;
    [SerializeField] private bool confineCursor;

    [Header("Frame Rate")]
    [SerializeField] private bool capFrameRate;
    [SerializeField] private int targetFrameRate = 60;

    // [Header("Cheats")]
    
    private void Awake()           
    {
        // Set the Singleton
        if (Instance != null && Instance != this)
        {
            // Already set, destroy this object
            Destroy(gameObject);
            return;
        }
        // Not set yet
        Instance = this;

        // Add SaveData to save event
        SaveManager.StartSavingEvent += SaveData;
    }

    private void Start()
    {
        // Load Data
        save = SaveManager.Instance;
        LoadData();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        SaveManager.StartSavingEvent -= SaveData;
    }

    public void SaveData()
    {
        // Save data to file
        save.SaveConfigData("capFrameRate", capFrameRate ? 1 : 0);
        save.SaveConfigData("hideCursor", hideCursor ? 1 : 0);
        save.SaveConfigData("lockCursor", lockCursor ? 1 : 0);
        save.SaveConfigData("confineCursor", confineCursor ?  1 : 0);
    }

    public void LoadData()
    {
        // load variables from data
        capFrameRate = save.LoadIntData("capFrameRate") == 1;
        hideCursor = save.LoadIntData("hideCursor") == 1;
        lockCursor = save.LoadIntData("lockCursor") == 1;
        confineCursor = save.LoadIntData("confineCursor") == 1;

        // Set variables
        if (capFrameRate) SetFrameRate(targetFrameRate);

        HideCursor(hideCursor);
        LockCursor(lockCursor);
        ConfineCursor(confineCursor);
    }

    /// <summary>
    /// Sets the target frame rate that the game will run at.
    /// </summary>
    /// <param name="frameRate"> Target frame rate </param>
    public void SetFrameRate(int frameRate)
    {
        Application.targetFrameRate = frameRate;
    }

    public void HideCursor(bool toggle)
    {
        Cursor.visible = !toggle;
    }

    public void LockCursor(bool toggle)
    {
        if (toggle)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    public void ConfineCursor(bool toggle)
    {
        if (toggle)
            Cursor.lockState = CursorLockMode.Confined;
        else
            Cursor.lockState = CursorLockMode.None;
    }
}
