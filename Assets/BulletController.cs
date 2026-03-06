using UnityEngine;

public class BulletController : GravSpecialObject
{
    public float speed;

    private Rigidbody2D rb2D;

    private bool isGrabbed;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.linearVelocity = transform.up * speed;
    }

    private void FixedUpdate()
    {
        if (transform.position.x < -50 || transform.position.x > 50 || transform.position.y < -50 || transform.position.y > 50)
        {
            Destroy(gameObject);
        }
    }

    public void OnGrab()
    {
        rb2D.linearVelocity = Vector2.zero;
        rb2D.gravityScale = 1;
        isGrabbed = true;
    }

    public void OnRelease()
    {
        rb2D.linearVelocity = transform.up * speed;
        rb2D.gravityScale = 0;
        isGrabbed = false;
    }
}
