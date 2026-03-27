using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
/* -----------------------------------------------------------
 * Author:
 * Max Rothenberger
 *
 * Modified By:
 * Cami Lee
 * Chandler Van
 */// --------------------------------------------------------

public class ExitDoor : MonoBehaviour
{
    [Tooltip("The level to load when the player enters this door.")]
    [SerializeField] private Level _levelToLoad;

    [Tooltip("The type of door, which determines how it interacts with the player.")]
    [SerializeField] private DoorType _type;

    [Header("Locked Door Attributes")]
    [SerializeField] private GameObject _lockedVisuals;

    private bool _isLocked;

    /// <summary>
    /// When non-null, overrides the normal lock state for IsLocked() checks.
    /// true  = forced locked   (door will never open)
    /// false = forced unlocked (door always opens, ignoring key state)
    /// null  = no override; use the real lock state
    /// </summary>
    private bool? _forcedState = null;

    private enum DoorType { Default, Locked }

    /// <summary>Sets the door's real lock state and updates visuals.</summary>
    public void Lock(bool locked)
    {
        _isLocked = locked;
        if (_lockedVisuals != null) _lockedVisuals.SetActive(locked);
    }

    /// <summary>
    /// Returns whether the door is currently considered locked,
    /// respecting any active forced state.
    /// </summary>
    public bool IsLocked()
    {
        return _forcedState ?? _isLocked;
    }

    /// <summary>
    /// Forces the door into a specific lock state, overriding key interactions
    /// until <see cref="ClearForcedState"/> is called.
    /// Pass <c>true</c> to force-lock, <c>false</c> to force-unlock.
    /// </summary>
    public void ForcedState(bool forceLocked)
    {
        _forcedState = forceLocked;

        // Keep visuals in sync with the effective state
        if (_lockedVisuals != null) _lockedVisuals.SetActive(IsLocked());
    }

    /// <summary>
    /// Removes any forced state so the door goes back to responding
    /// normally to key interactions.
    /// </summary>
    public void ClearForcedState()
    {
        _forcedState = null;

        // Restore visuals to the real underlying lock state
        if (_lockedVisuals != null) _lockedVisuals.SetActive(_isLocked);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool canOpen = false;
        if (_type == DoorType.Locked && !IsLocked()) canOpen = true;
        else if (_type == DoorType.Default) canOpen = true;

        if (canOpen && collision.CompareTag("Player")) StartCoroutine(nameof(EndLevel));
    }

    private IEnumerator EndLevel()
    {
        Debug.Log("Level Complete");
        yield return new WaitForSeconds(2f);

        NextLevel(_levelToLoad);
    }

    public void NextLevel(Level level)
    {
        LevelManager.Instance.LoadLevelViaLevelName(level);
    }
}