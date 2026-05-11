using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;

    private void Update()
    {
        transform.Translate(Vector3.up*Time.deltaTime*speed);
    }

    public void End()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(1);
            End();
        }
    }

}
