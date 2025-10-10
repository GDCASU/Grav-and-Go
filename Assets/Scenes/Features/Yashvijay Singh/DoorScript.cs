using UnityEngine;

public class DoorScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int _rotateAmt = 0;
    private bool _doorXrotate =false;
    private bool _doorYrotate = false;
    public bool PlayerCloseEnough = false;
    private float _closeEnoughDistance = 1;
    public PlayerMovementController Player;  

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
        
        if (PlayerCloseEnough)
        {
            doorRotateY();
        }
    }

    void doorRotateY() // make door rotate 90 degrees about the y axis
    {
        _rotateAmt = 45;
        transform.Rotate(Vector3.up, _rotateAmt);
        
    }
}
