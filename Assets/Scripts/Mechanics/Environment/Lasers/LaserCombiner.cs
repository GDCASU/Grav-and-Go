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
/// </summary>
public class LaserCombiner : MonoBehaviour
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    
    [Header("Sensors")]
    [SerializeField] private GameObject ColorSens1 = null; 
    [SerializeField] private GameObject ColorSens2 = null;
    // Start is called before the first frame update
    void Start()
    {
        if(ColorSens1 == null) Debug.LogError("Color Sensor 1 not defined for laser combiner named " + this.name);
        if(ColorSens2 == null) Debug.LogError("Color Sensor 2 not defined for laser combiner named " + this.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
