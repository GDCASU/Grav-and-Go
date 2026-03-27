using System;
using UnityEngine;
/* -----------------------------------------------------------
 * Author:
 * Max Rothenberger
 * 
 * Modified By:
 * Cami Lee
 * 
 */// --------------------------------------------------------

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class DoorKey : MonoBehaviour
{
    [Tooltip("The door this key is assigned to open")]
    [SerializeField] private ExitDoor _assignedDoor;
    private Collider2D _doorColl;

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
        if (_assignedDoor == null) Debug.LogError($"Assign a door to the key {this}");
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalPos = transform.position;
        _doorColl = _assignedDoor.GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (_followMode == FollowMode.Follow)
        {
            float x = _followTarget.position.x - Mathf.Sign(_followTarget.localScale.x);
            float y = _followTarget.position.y + 0.5f;
            Vector2 behindTarget = new Vector3(x, y);
            transform.position = Vector2.Lerp(transform.position, behindTarget, Time.fixedDeltaTime * 5f);

            // Unlock the door if the key is close enough
            if (Vector2.Distance(_followTarget.position, _doorColl.ClosestPoint(_followTarget.position)) < 1f) Unlock();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (_followTarget != null) return;
            _followTarget = collision.transform;
            _followMode = FollowMode.Follow;
        }
    }

    /// <summary> Unlocks the door and makes the key disappear </summary>
    private void Unlock()
    {
        Debug.Log("Unlock");
        _assignedDoor.Lock(false);

        _collider.enabled = false;
        _spriteRenderer.enabled = false;

        _followMode = FollowMode.None;
    }

    /// <summary>
    /// Resets the key
    /// </summary>
    public void ResetObject()
    {
        transform.position = _originalPos;

        _assignedDoor.Lock(true);

        _collider.enabled = true;
        _spriteRenderer.enabled = true;

        _followMode = FollowMode.None;
    }
}
