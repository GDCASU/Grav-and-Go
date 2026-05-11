using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 *  Pablo Pelaez Fundora
 *
 * Modified By:
 *
 *
 */// --------------------------------------------------------


/// <summary>
/// Class that handles the functionalities of the bear trap hazard
/// </summary>

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class BearTrap : MonoBehaviour
{
    private static readonly int TriggerHash = Animator.StringToHash("Trigger");

    [Header("References")]

    [Header("TrapStats")]
    [SerializeField] private float trapVelocity = 10f;
    [Tooltip("A cosmetic feature that makes the trap jump upwards when activated")]
    [SerializeField] private float trapSelfJumpForce = 1f;

    [Header("Debugging")]
    [SerializeField] bool _doDebugLog = false;

    private new Collider2D collider;
    private new Rigidbody2D rigidbody;
    private Animator animator;


    private void Awake()
    {
        collider = GetComponent<Collider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {

        // TODO: close animation 
        // TODO: close sound effect

        if (other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            return;
        
        // disable the collider to make sure it can't be used again
        collider.enabled = false;

        animator.SetTrigger(TriggerHash);

        rigidbody.AddForce(transform.up * trapSelfJumpForce, ForceMode2D.Impulse);
        rigidbody.AddTorque(UnityEngine.Random.Range(-0.5f, 0.5f), ForceMode2D.Impulse);

        ApplyVelocityToObject(other.gameObject);


        if (other.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(100000);
        }
    }

    

    // Grabs the velocity of the collided object and modifies it so it flies up
    private void ApplyVelocityToObject(GameObject objectToApply)
    {
        if(objectToApply.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + trapVelocity);
        }

        if (_doDebugLog) Debug.Log($"Applying velocity to {objectToApply.name}");
    }
}
