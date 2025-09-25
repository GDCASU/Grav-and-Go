using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Rika Vuong
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------
public class ResultsMenuSuccess : MonoBehaviour 
{
    // stats displayed on the menu
    private float timer = 0f;
    int health;
    string powerUps;
    void Start()
    {

    }

    void Update()
    {
        timer += Time.deltaTime;
    }
}
