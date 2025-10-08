using UnityEngine;

public class DoorWithLock : MonoBehaviour
{
    private bool _isEnabled;

    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Lock(bool locked)
    {
        _collider.enabled = locked;
        _spriteRenderer.enabled = locked;
    }
}
