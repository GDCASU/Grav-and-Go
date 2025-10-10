using UnityEngine;

public class DoorScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int _rotateAmt = 0;
    private bool _doorXrotate =false;
    private bool _doorYrotate = false;
    public bool PlayerCloseEnough = false;
    private float _closeEnoughDistance = 2;
    private int _countClose = 0; // used so door only rotates once when player is near
    private int _countFar = 1; // used to rotate door back when player no longer close
    public PlayerMovementController Player;  

    // now give it collision i guess? 

    void Start()
    {
        Player = FindFirstObjectByType<PlayerMovementController>();
      

    }

    // Update is called once per frame
    void Update()
    {

        if (Vector2.Distance(Player.transform.position, transform.position) < _closeEnoughDistance)//checks if player close to door
        {
            PlayerCloseEnough = true;

        }
        else
        {
            PlayerCloseEnough = false;
        }
        

        if (PlayerCloseEnough && (_countClose < 1))
        {
            doorRotateY();
            _countClose += 1;
            _countFar = 0;

        }
        if (!PlayerCloseEnough && (_countFar < 1))
        {
            doorRotateY();
            _countClose = 0;
            _countFar += 1;
        }
    }

    void doorRotateY() // make door rotate 90 degrees about the y axis
    {
        _rotateAmt = 90;
        transform.Rotate(Vector3.up, _rotateAmt);
        
    }
}
