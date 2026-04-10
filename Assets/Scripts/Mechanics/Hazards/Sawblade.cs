using UnityEngine;

public class Sawblade : MonoBehaviour
{
    private int damage = 0;
    
    public void Init(int damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(damage);
    }
}
