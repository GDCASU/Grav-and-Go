using UnityEngine;

public class SpawnDoor : MonoBehaviour
{
    private PlayerMovementController _player;

    [SerializeField] private Vector2 _offset; //In case you do not want the player spawning at the door's center.

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerMovementController>();

        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        _player.transform.position = transform.position + (Vector3)_offset;
    }
}
