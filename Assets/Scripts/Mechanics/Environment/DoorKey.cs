using UnityEngine;

public class DoorKey : MonoBehaviour
{
    [SerializeField] private DoorWithLock _assignedDoor;

    private Vector3 _originalPos;

    private Transform followTarget;

    private FollowMode _followMode;
    private enum FollowMode
    {
        None,
        Follow
    }

    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (_followMode == FollowMode.Follow)
        {
            transform.position = Vector2.Lerp(transform.position, followTarget.position, Time.fixedDeltaTime);
            if (Vector2.Distance(transform.position, _assignedDoor.transform.position) < 2f) Unlock();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerMovementController player))
        {
            if (followTarget != null) return;
            followTarget = player.transform;
        }
    }

    private void Unlock()
    {
        _assignedDoor.Lock(false);

        _collider.enabled = false;
        _spriteRenderer.enabled = false;

        _followMode = FollowMode.None;
    }

    private void ResetObject()
    {
        transform.position = _originalPos;

        _assignedDoor.Lock(true);

        _collider.enabled = true;
        _spriteRenderer.enabled = true;

        _followMode = FollowMode.None;
    }
}
