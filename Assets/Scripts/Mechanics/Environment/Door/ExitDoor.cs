using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
/* -----------------------------------------------------------
 * Author:
 * Max Rothenberger
 * 
 * Modified By:
 * Cami Lee (to support locked doors)
 * 
 */// --------------------------------------------------------

public class ExitDoor : MonoBehaviour
{
    [SerializeField] private Level _levelToLoad;
    [SerializeField] DoorType type;

    [Header("Locked Door Attributes")]
    private DoorWithLock doorWithLock;
    enum DoorType { Default, Locked }

    private void Start()
    {
        if (type == DoorType.Locked) doorWithLock = GetComponent<DoorWithLock>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check to see if the door can be opened
        bool canOpen = false;
        if (type == DoorType.Locked && !doorWithLock.IsLocked()) canOpen = true;
        else if (type == DoorType.Default) canOpen = true;

        // Ensures the level does not end when a non-player object touches the door.
        if (canOpen && collision.CompareTag("Player")) StartCoroutine(nameof(EndLevel));
    }

    private IEnumerator EndLevel()
    {
        //Do code that occurs prior to loading the next level.

        Debug.Log("Level Complete");
        yield return new WaitForSeconds(2f); //Add a delay in the case of transitions or having something to read.

        NextLevel(_levelToLoad);
    }

    public void NextLevel(Level level)
    {
        LevelManager.Instance.LoadLevelViaLevelName(level);
    }
}

