using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Modified by: Joshua Wright (2026)
 * ----------------------------------------------------------- */

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
        [Tooltip("Collision mask of the object to BE DAMAGED")]
        public LayerMask mask;

        [Tooltip("Collision type")]
        public CollisionType type;

        [Tooltip("Damage to deal to the target")]
        public int damage;

        [Tooltip("Do Debug Log")]
        public bool debug = true;
    }

    [SerializeField] private List<LayerEventPair> layerCollisionEvents = new();

    // Note: rgd is usually the source of the impact for knockback logic
    private Rigidbody2D myRgd;

    private void Start()
    {
        myRgd = GetComponent<Rigidbody2D>();
    }

    // Collision Handlers
    private void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision.gameObject, CollisionType.Enter);
    private void OnCollisionStay2D(Collision2D collision) => HandleCollision(collision.gameObject, CollisionType.Stay);
    private void OnCollisionExit2D(Collision2D collision) => HandleCollision(collision.gameObject, CollisionType.Leave);

    // Trigger Handlers
    private void OnTriggerEnter2D(Collider2D collision) => HandleCollision(collision.gameObject, CollisionType.Enter);
    private void OnTriggerStay2D(Collider2D collision) => HandleCollision(collision.gameObject, CollisionType.Stay);
    private void OnTriggerExit2D(Collider2D collision) => HandleCollision(collision.gameObject, CollisionType.Leave);

    // Core logic
    public void HandleCollision(GameObject other, CollisionType currentType)
    {
        int layer = other.layer;

        foreach (var pair in layerCollisionEvents)
        {
            // Check if the layer matches the mask AND the collision phase matches
            if (((1 << layer) & pair.mask.value) != 0 && pair.type == currentType)
            {
                // We find the interface on the 'other' object, not 'this'
                if (other.TryGetComponent<IDamageable>(out IDamageable targetDamageable))
                {
                    if (pair.debug)
                    {
                        Debug.Log($"<b><color=orange>{gameObject.name}</color></b> dealt {pair.damage} damage to <b><color=red>{other.name}</color></b> via {currentType}");
                    }

                    // Apply damage to the thing we hit
                    // We pass myRgd as the 'source' of the force for knockback
                    targetDamageable.TakeDamage(pair.damage, myRgd);
                }
            }
        }
    }
}