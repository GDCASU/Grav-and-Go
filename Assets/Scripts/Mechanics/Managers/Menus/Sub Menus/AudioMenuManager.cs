using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Class that handles an audio menu
/// </summary>
public class AudioMenuManager : MonoBehaviour
{
    [Header("Audio")] 
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TextMeshProUGUI masterValue;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicValue;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private TextMeshProUGUI SFXValue;
    
    [Header("Optional")]
    [SerializeField] private TextMeshProUGUI masterShadow;
    [SerializeField] private TextMeshProUGUI musicShadow;
    [SerializeField] private TextMeshProUGUI SFXShadow;
    
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool doDebugLog;
    
    void Start()
    {
        // On load, set the loaded sound values into the audio sliders.
        // GetVCAVolume returns [0,1], so scale up to each slider's maxValue
        // to match the display range (e.g. 0-100).
        float volume = 0f;
        string volumeStr = "";
        volume = SoundManager.Instance.GetVCAVolume(SoundControllers.Master) * masterSlider.maxValue;
        volumeStr = volume.ToString("F0");
        masterSlider.value = volume;
        masterValue.text = volumeStr;
        if (masterShadow != null) masterShadow.text = volumeStr;
        volume = SoundManager.Instance.GetVCAVolume(SoundControllers.Music) * musicSlider.maxValue;
        volumeStr = volume.ToString("F0");
        musicSlider.value = volume;
        musicValue.text = volumeStr;
        if (musicShadow != null) musicShadow.text = volumeStr;
        volume = SoundManager.Instance.GetVCAVolume(SoundControllers.SFX) * SFXSlider.maxValue;
        volumeStr = volume.ToString("F0");
        SFXSlider.value = volume;
        SFXValue.text = volumeStr;
        if (SFXShadow != null) SFXShadow.text = volumeStr;
    }
    
    /// <summary>
    /// Changes the value of the master volume to the slider val
    /// </summary>
    public void ChangeMasterVolume()
    {
        float sliderVal = masterSlider.value;
        string sliderValStr = sliderVal.ToString("F0");
        if (masterShadow != null) masterShadow.text = sliderValStr;
        masterValue.text = sliderValStr;
        SoundManager.Instance.SetVolume(SoundControllers.Master, sliderVal, masterSlider.maxValue);
    }
    
    /// <summary>
    /// Changes the value of the music volume to the slider val
    /// </summary>
    public void ChangeMusicVolume()
    {
        float sliderVal = musicSlider.value;
        string sliderValStr = sliderVal.ToString("F0");
        if (musicShadow != null) musicShadow.text = sliderValStr;
        musicValue.text = sliderValStr;
        SoundManager.Instance.SetVolume(SoundControllers.Music, sliderVal, musicSlider.maxValue);
    }
    
    /// <summary>
    /// Changes the value of the SFX volume to the slider val
    /// </summary>
    public void ChangeSFXVolume()
    {
        float sliderVal = SFXSlider.value;
        string sliderValStr = sliderVal.ToString("F0");
        if (SFXShadow != null) SFXShadow.text = sliderValStr;
        SFXValue.text = sliderValStr;
        SoundManager.Instance.SetVolume(SoundControllers.SFX, sliderVal, SFXSlider.maxValue);
    }
}