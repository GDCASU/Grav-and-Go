using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// 
/// </summary>
public class FPSCounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI targetText;
    
    [Header("Settings")]
    [Tooltip("Average over this many frames for a smoother readout")]
    [SerializeField, Range(1, 120)] private int sampleSize = 30;
    
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

    [Header("Readouts")]
    [SerializeField, InspectorReadOnly] private bool _enabledReadout;
    
    // Local Variables
    public static bool enable = true;   
    private float[] frameTimes;
    private int index;

    private void Awake()
    {
        // Initialize array
        frameTimes = new float[sampleSize];
    }

    void Update()
    {
        // Dont do anything if not enabled
        _enabledReadout = enable;
        if (!enable) 
        {
            targetText.gameObject.SetActive(false);
            return;
        }
        
        targetText.gameObject.SetActive(true);
        
        // circular buffer of deltaTimes
        frameTimes[index] = Time.unscaledDeltaTime;
        index = (index + 1) % sampleSize;

        // calculate average
        float sum = 0f;
        for (int i = 0; i < sampleSize; i++) sum += frameTimes[i];
        float fps = 1f / (sum / sampleSize);

        targetText.text = $"FPS: {fps:0.#}";
    }
}
