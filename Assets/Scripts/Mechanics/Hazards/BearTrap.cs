using System;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 *  Pablo Pelaez Fundora
 *
 * Modified By:
 *
 *
 */// --------------------------------------------------------


/// <summary>
/// Class that handles the functionalities of the bear trap hazard
/// </summary>

public class BearTrap : MonoBehaviour
{   
    [Header("Debugging")] 
    [SerializeField] bool _doDebugLog = false;
    
    [Header("TrapStats")]
    Collider2D bearTrapColl;
    [SerializeField] private float trapVelocity = 10f;

    void Start()
    {
        bearTrapColl = GetComponent<Collider2D>();
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {   
        // close animation 
        // close sound effect
        bearTrapColl.enabled = false;
        
        if (other.gameObject.tag == "Player")
        {
            // I have no idea where I can use a game over code. I just used the level manager for now
            // add delay if needed unless that is part of the manager
            LevelManager.Instance.LoadLastCheckpoint(); 
            
        }
        else if (other.gameObject.tag == "Physics Object")
        {
            ApplyVelocityToObject(other.gameObject);
        }
    }

    

    // Grabs the velocity of the collided object and adds to it
    private void ApplyVelocityToObject(GameObject objectToApply)
    {
        Rigidbody2D rb = objectToApply.GetComponent<Rigidbody2D>();
        if (rb != null){rb.linearVelocity = new Vector2(0,rb.linearVelocity.y+trapVelocity);}
        if (_doDebugLog) Debug.Log($"Applying velocity to {objectToApply.name}");
    }
}
