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
public class LaserSensor : MonoBehaviour
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    
    [Header("Laser Color")]
    [SerializeField] private LaserColorEnum requiredColor = LaserColorEnum.Red;

    [Header("Events")]
    public UnityEvent laserSensorActive;
    public UnityEvent laserSensorDisabled;
    private Coroutine sensorOff = null;

    public void laserHit(LaserColorEnum laserCol)
    {
        if(laserCol == requiredColor)
        {
            laserSensorActive.Invoke();
            if(sensorOff != null)
            {
                StopCoroutine(sensorOff);
            }
            sensorOff = StartCoroutine(disableSensor());
        }
    }

    IEnumerator disableSensor()
    {
        yield return new WaitForSeconds(1);
        laserSensorDisabled.Invoke();
        sensorOff = null;
    }
}
