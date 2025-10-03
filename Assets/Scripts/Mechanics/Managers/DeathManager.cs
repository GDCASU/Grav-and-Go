using UnityEngine;
using UnityEngine.Events;

/* -----------------------------------------------------------
 * Author:
 * Finn Meeks
 *
 * Modified By:
 * 
 */// --------------------------------------------------------


/// <summary>
/// Class that handles death. Initiate the player death with TriggerPlayerDeath()
/// </summary>
public class DeathManager : MonoBehaviour
{
    // Singleton
    public static DeathManager Instance { get; private set; }

    [SerializeField] public UnityEvent OnPlayerDeath = new UnityEvent();

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


    public void TriggerPlayerDeath()
    {
        GameObject player = Object.FindObjectOfType<PlayerMovementController>().gameObject;

        if (player != null)
        {
            OnPlayerDeath.Invoke();
            Destroy(player);
        }
    }
}