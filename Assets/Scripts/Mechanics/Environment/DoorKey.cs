using System;
using UnityEngine;

public class DoorKey : MonoBehaviour
{
    [SerializeField] private DoorWithLock _assignedDoor;

    private Vector3 _originalPos;

    private Transform _followTarget;

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
            float x = _followTarget.position.x - Mathf.Sign(_followTarget.localScale.x);
            float y = _followTarget.position.y + 0.5f;
            Vector2 behindTarget = new Vector3(x, y);
            transform.position = Vector2.Lerp(transform.position, behindTarget, Time.fixedDeltaTime * 5f);

            Collider2D col = _assignedDoor.GetComponent<Collider2D>();
            if (Vector2.Distance(_followTarget.position, col.ClosestPoint(_followTarget.position)) < 1f) Unlock();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerMovementController player))
        {
            if (_followTarget != null) return;
            _followTarget = player.transform;
            _followMode = FollowMode.Follow;
        }
    }

    private void Unlock()
    {
        Debug.Log("Unlock");
        _assignedDoor.Lock(false);

        _collider.enabled = false;
        _spriteRenderer.enabled = false;

        _followMode = FollowMode.None;
    }

    public void ResetObject()
    {
        transform.position = _originalPos;

        _assignedDoor.Lock(true);

        _collider.enabled = true;
        _spriteRenderer.enabled = true;

        _followMode = FollowMode.None;
    }
}
