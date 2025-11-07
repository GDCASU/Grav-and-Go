using UnityEngine;
/* -----------------------------------------------------------
 * Author:
 * Max Rothenberger
 * 
 * Modified By:
 * Cami Lee (to support exit doors)
 * 
 */// --------------------------------------------------------

public class DoorWithLock : MonoBehaviour
{
    private bool _isEnabled;

    [SerializeField] SpriteRenderer _lockSprite;

    public void Lock(bool locked)
    {
        _lockSprite.enabled = locked;
        _isEnabled = locked;
    }

    public bool IsLocked()
    {
        return _isEnabled;
    }
}
