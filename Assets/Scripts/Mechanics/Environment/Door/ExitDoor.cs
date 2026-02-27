using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitDoor : MonoBehaviour
{
    [SerializeField] private Level _levelToLoad;
    [SerializeField] DoorType type;
    [SerializeField] private LevelTimer timer;

    [Header("Locked Door Attributes")]
    private DoorWithLock doorWithLock;
    enum DoorType { Default, Locked }

    private void Start()
    {
        if (type == DoorType.Locked) doorWithLock = GetComponent<DoorWithLock>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool canOpen = false;
        if (type == DoorType.Locked && !doorWithLock.IsLocked()) canOpen = true;
        else if (type == DoorType.Default) canOpen = true;

        if (canOpen && collision.CompareTag("Player")) StartCoroutine(nameof(EndLevel));
    }

    private IEnumerator EndLevel()
    {
        if (timer != null)
        {
            timer.ToggleTimer(false); // Stop the clock so the time doesn't keep running during the 2s delay
            
            // Pass the final time to the LevelManager to check for a new "Best Time"
            Debug.Log("Update Level Time Triggered");
            LevelManager.Instance.UpdateLevelTime(timer.TotalTime);
        }

        Debug.Log("Level Complete");
        yield return new WaitForSeconds(2f); 

        NextLevel(_levelToLoad);
    }

    public void NextLevel(Level level)
    {
        LevelManager.Instance.LoadLevelViaLevelName(level);
    }
}