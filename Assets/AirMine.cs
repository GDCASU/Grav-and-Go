using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

public class AirMine : MonoBehaviour
{
    public float explodeRadius;
    public float detectRadius;

    private void Awake() {
        GetComponent<CircleCollider2D>().radius = detectRadius;
        var shape = GetComponent<ParticleSystem>().shape;
        shape.radius = detectRadius;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("COLLISION1");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        
        for(int i = 0; i<colliders.Length; i++)
        {
            //deal damage to these objects
        }

        Destroy(gameObject);
    }
}
