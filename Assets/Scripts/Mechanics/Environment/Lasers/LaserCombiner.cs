using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Davyd Yehudin
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Defines LaserCombiner object
/// also please understand that i was under a timecrunch and was feeling unwell
/// so please don't judge the code quality and the logic quality
/// </summary>
public class LaserCombiner : MonoBehaviour
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    // Start is called before the first frame update
    private LaserColorEnum Color1, Color2 = LaserColorEnum.None;
    private LaserPointer LaserScript;
    private Coroutine ColorTimerRef1, ColorTimerRef2;
    private void Awake()
    {
        LaserScript = this.GetComponent<LaserPointer>();
        LaserScript.SetLaserSwitch(false);
    }
    void Update()
    {
        if(ColorTimerRef1 != null && ColorTimerRef2 != null)
        {
            //if(_doDebugLog) Debug.Log(Color1 + " " + Color2);
            TurnOnTheLaser();
        }
        else
        {
            LaserScript.SetLaserSwitch(false);
        }
    }
    void TurnOnTheLaser()
    {
        int laserColorID = 1;
        int Color1ID = (int)(Color1);
        int Color2ID = (int)(Color2); 
        if((Color1ID == (int)(LaserColorEnum.Red) || Color1ID == (int)(LaserColorEnum.Lime) || Color1ID == (int)(LaserColorEnum.Cyan)) &&
        (Color2ID == (int)(LaserColorEnum.Red) || Color2ID == (int)(LaserColorEnum.Lime) || Color2ID == (int)(LaserColorEnum.Cyan)) &&
        Color1ID != Color2ID)
        {
            laserColorID = (int)(Color1) * (int)(Color2);
        }
        else
        {
            return;
        }
        LaserScript.SetLaserSwitch(true);
        LaserScript.laserColor = (LaserColorEnum)laserColorID;
    }
    public void OnLaserHit(LaserColorEnum color, float tickSpeed)
    {
        float waitMult = 3f;
        if(Color1 == LaserColorEnum.None || ColorTimerRef1 == null)
        {
            Color1 = color;
            ColorTimerRef1 = StartCoroutine(ColorTimer1( tickSpeed * waitMult ) );
            return;
        }
        if(Color1 == color)
        {
            StopCoroutine(ColorTimerRef1);
            ColorTimerRef1 = StartCoroutine(ColorTimer1( tickSpeed * waitMult ) );
            return;
        }
        if(Color2 == LaserColorEnum.None || ColorTimerRef2 == null)
        {
            Color2 = color;
            ColorTimerRef2 = StartCoroutine(ColorTimer2( tickSpeed * waitMult ) );
            return;
        }
        if(Color2 == color)
        {
            StopCoroutine(ColorTimerRef2);
            ColorTimerRef2 = StartCoroutine(ColorTimer2( tickSpeed * waitMult ) );
            return;
        }
    }

    private IEnumerator ColorTimer1(float time)
    {
        //if(_doDebugLog) Debug.Log("Start 1");
        yield return new WaitForSeconds(time);
        if(_doDebugLog) Debug.Log("Null now 1");
        ColorTimerRef1 = null;
    }

    private IEnumerator ColorTimer2(float time)
    {
        //if(_doDebugLog) Debug.Log("Start 2");
        yield return new WaitForSeconds(time);
        if(_doDebugLog) Debug.Log("Null now 2");
        ColorTimerRef2 = null;
    }
}
