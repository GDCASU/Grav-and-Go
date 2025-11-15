using UnityEngine;
/* -----------------------------------------------------------
 * Author:
 * Cami Lee
 * 
 * Modified By:
 * 
 * 
 */// --------------------------------------------------------


/// <summary>
/// Class that handles the functionalities of the checkpoint system
/// </summary>
public class Checkpoint : MonoBehaviour
{
    bool hasSaved;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !hasSaved)
        {
            LevelManager.Instance.SaveCheckpoint();
            hasSaved = true;

            // TBI: Some sort of UI loading circle
        }
    }
}
