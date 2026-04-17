using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float _jumpForce; //Regular force to launch objects.
    [SerializeField] private float _playerMultiplier; //Special modifier to launch the player since it uses custom physics.

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Rigidbody2D rb))
        {
            rb.AddForce(transform.up * _jumpForce, ForceMode2D.Impulse);
        }

        if (collision.gameObject.TryGetComponent(out PlayerMovementController player))
        {
            player.AddVelocity(transform.up * _jumpForce * _playerMultiplier);
        }
    }
}
