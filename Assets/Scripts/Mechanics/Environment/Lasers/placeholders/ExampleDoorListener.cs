using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * 
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// 
/// </summary>
public class ExampleDoorListener : MonoBehaviour
{

    void Awake()
    {
        close();
    }
    public void open()
    {
        this.GetComponent<SpriteRenderer>().color = Color.green;
        Debug.Log("door open");
    }

    public void close()
    {
        this.GetComponent<SpriteRenderer>().color = Color.red;
        Debug.Log("door close");
    }
}
