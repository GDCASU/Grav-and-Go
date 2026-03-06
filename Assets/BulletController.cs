using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] public float speed;

    [SerializeField] private Rigidbody2D rb2D;

    private bool _isGrabbed;

    private void FixedUpdate()
    {
        if (!_isGrabbed)
        {
            rb2D.linearVelocity = transform.up * speed;
        }
        else
        {
            
        }
    }
}
