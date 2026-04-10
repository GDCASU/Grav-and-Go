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
    [SerializeField] private bool needCertainColor = true;
    [SerializeField] private LaserColorEnum requiredColor = LaserColorEnum.Red;
    [Header("Laser Timings")]
    [SerializeField] private float TimeToTurnOff = 0.5f;

    [Header("Events")]
    public UnityEvent laserSensorActive;
    public UnityEvent laserSensorDisabled;
    private Coroutine sensorOff = null;

    public void LaserHit(LaserColorEnum laserCol)
    {
        if(!needCertainColor || laserCol == requiredColor)
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
        yield return new WaitForSeconds(TimeToTurnOff);
        laserSensorDisabled.Invoke();
        sensorOff = null;
    }
}
