using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(CircleCollider2D))]
public class AirMine : MonoBehaviour
{
    [Tooltip("The damage this mine will deal to objects in explosion radius")]
    public int damage;
    [Tooltip("The radius in which this mine will deal damage to objects")]
    public float explodeRadius;
    [Tooltip("The radius in which this mine will detect objects to explode")]
    public float detectRadius;
    ParticleSystem.ShapeModule shape;

    private void Awake() {
        shape = GetComponent<ParticleSystem>().shape;
    }

    private void Start()
    {
        GetComponent<CircleCollider2D>().radius = detectRadius;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        shape.radius = detectRadius;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        
        foreach(Collider2D collider in colliders)
        {
            if(!collider.TryGetComponent<IDamageable>(out IDamageable damageable)) continue;

            damageable.TakeDamage(damage);
        }

        // Note: may want to replace with an explosion animation later
        Destroy(gameObject);
    }
}
