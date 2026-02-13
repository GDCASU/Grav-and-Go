using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* -----------------------------------------------------------
 * Author:
 * Davyd Yehudin
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// 
/// </summary>
public class ColorSensor : MonoBehaviour
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    
    [Header("Events")]
    public UnityEvent<LaserColorEnum> laserSensorActive;
    public UnityEvent laserSensorDisabled;
    private Coroutine sensorOff = null;

    public void laserHit(LaserColorEnum laserCol)
    {
        laserSensorActive.Invoke(laserCol);
        if(sensorOff != null)
        {
            StopCoroutine(sensorOff);
        }
        sensorOff = StartCoroutine(disableSensor());
        
    }

    IEnumerator disableSensor()
    {
        yield return new WaitForSeconds(1);
        laserSensorDisabled.Invoke();
        sensorOff = null;
    }
}
