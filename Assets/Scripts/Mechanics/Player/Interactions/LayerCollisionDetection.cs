using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
 *  Matthew Glos 9/12/25
 *  
 *  General purpose script to detect collisions with objects of a particular layer 
 *  and call a set of UnityEvents when those collisions occur.
 *  
 *  Written to handle the player object colliding with objects with the Hazard tag
 *  to trigger the death sequence. 
 *  
 *  Attach to the object you'd like to detect the collisions, and make sure either 
 *  a collision or trigger event can happend between the two objects
 */

public class LayerCollisionDetection : MonoBehaviour
{
    [System.Serializable]
    public enum CollisionType
    {
        Enter,
        Stay,
        Leave
    }

    [System.Serializable]
    public class LayerEventPair
    {
        [Tooltip("Collision mask")]
        public LayerMask mask;

        [Tooltip("Collision type")]
        public CollisionType type;

        [Tooltip("Collision event")]
        public UnityEvent events;

        [Tooltip("Do Debug Log")]
        public bool debug;
    }

    [SerializeField] private List<LayerEventPair> layerCollisionEvents = new();

    private void OnCollisionEnter2D(Collision2D collision) =>
        HandleCollision(collision.gameObject.layer, CollisionType.Enter);

    private void OnCollisionStay2D(Collision2D collision) =>
        HandleCollision(collision.gameObject.layer, CollisionType.Stay);

    private void OnCollisionExit2D(Collision2D collision) =>
        HandleCollision(collision.gameObject.layer, CollisionType.Leave);

    private void OnTriggerEnter2D(Collider2D collision) =>
        HandleCollision(collision.gameObject.layer, CollisionType.Enter);

    private void OnTriggerStay2D(Collider2D collision) =>
        HandleCollision(collision.gameObject.layer, CollisionType.Stay);

    private void OnTriggerExit2D(Collider2D collision) =>
        HandleCollision(collision.gameObject.layer, CollisionType.Leave);

    // Core logic
    public void HandleCollision(int layer, CollisionType currentType)
    {
        foreach (var pair in layerCollisionEvents)
        {
            if (((1 << layer) & pair.mask.value) != 0 && pair.type == currentType)
            {
                if (pair.debug)
                {
                    string pr = $"<b><color=red>{this.gameObject.name} {currentType} with layer {LayerMask.LayerToName(layer)}</color></b>";
                    Debug.Log(pr);
                }
                pair.events?.Invoke();
            }
        }
    }
}
