using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitDoor : MonoBehaviour
{
    [SerializeField] private Level _thisLevel; //I currently do not know how to get this to pass to the level manager.

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Ensures the level does not end when a non-player object touches the door.
        if (collision.gameObject.GetComponent<PlayerMovementController>()) StartCoroutine(nameof(EndLevel));
    }

    private IEnumerator EndLevel()
    {
        //Do code that occurs prior to loading the next level.

        Debug.Log("Level Complete");
        yield return new WaitForSeconds(2f); //Add a delay in the case of transitions or having something to read.

        NextLevel(_thisLevel);
    }

    public void NextLevel(Level level)
    {
        LevelManager.Instance.LoadLevelViaLevelName(level);
    }
}
