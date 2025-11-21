using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

public class AirMine : MonoBehaviour
{
    public float explodeRadius;
    public float detectRadius;
    ParticleSystem.ShapeModule shape;

    private void Awake() {
        GetComponent<CircleCollider2D>().radius = detectRadius;
        shape = GetComponent<ParticleSystem>().shape;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        shape.radius = detectRadius;

        Debug.Log("COLLISION1");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        
        for(int i = 0; i<colliders.Length; i++)
        {
            //deal damage to these objects
            if (colliders[i].CompareTag("Player")) DeathManager.TriggerPlayerDeath();
        }

        // Note: may want to replace with an explosion animation later
        Destroy(gameObject);
    }
}
