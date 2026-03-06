using UnityEngine;
using UnityEngine.Events;

/* -----------------------------------------------------------
 * Author:
 * Finn Meeks
 *
 * Modified By:
 * Cami Lee
 * 
 */// --------------------------------------------------------


/// <summary>
/// Class that handles death. Initiate the player death with TriggerPlayerDeath()
/// Does not need to be added to the scene: can be called from anywhere
/// </summary>
public static class DeathManager
{
    private static bool _isPlayerDead = false;
    public static void TriggerPlayerDeath()
    {
        // If we are already dead, exit immediately!
        if (_isPlayerDead) return;

        _isPlayerDead = true;
        PlayerAnimator playerAnim = Object.FindFirstObjectByType<PlayerAnimator>();
        PlayerMovementController playerController = Object.FindFirstObjectByType<PlayerMovementController>();

        if (playerAnim != null)
        {
            // Player animation death & prevent from moving
            playerController.FreezeMovement();
            playerController.enabled = false;
            playerAnim.OnDeath();

            // Load death screen (TBI)

            // Reload current checkpoint after 3 seconds
            // (after player animation is done)
            LevelManager.Instance.LoadLastCheckpoint(3);
        }
    }
    
    public static void ResetDeathState()
    {
        _isPlayerDead = false;
    }
}