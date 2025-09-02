using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/* -----------------------------------------------------------
 * Author:
 * 
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Purpose:
 * 
 */// --------------------------------------------------------


/// <summary>
/// 
/// </summary>
public class Outline2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Material _outlineMaterial;
    
    [Header("Settings")]
    [SerializeField, Range(0f,50f)] private float _OutlineSizePx = 5f;
    [SerializeField] private Color _outlineColor = Color.green;
    
    [Header("Buttons")]
    [SerializeField] private bool _applySettings;
    [SerializeField] private bool _enableOutline;
    [SerializeField] private bool _disableOutline;
    
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    
    // Local variables
    private Material _localOutlineMat;
    
    // Hashes
    private static readonly int _OutlineSizePxID = Shader.PropertyToID("_OutlineSizePx");
    private static readonly int outlineColorID = Shader.PropertyToID("_Outline_Color");
    private static readonly int OutlineOnID = Shader.PropertyToID("_Outline_On");
    
    void Start()
    {
        // Replace sprite renderer material with outline shader
        _spriteRenderer.material = _outlineMaterial;
        _localOutlineMat = _spriteRenderer.material;
        ApplySettings();
    }

    // Check inspector buttons
    void Update()
    {
        if (_applySettings)
        {
            ApplySettings();
            _applySettings = false;
        }

        if (_enableOutline)
        {
            SetOutline(true);
            _enableOutline = false;
        }

        if (_disableOutline)
        {
            SetOutline(false);
            _disableOutline = false;
        }
    }

    private void ApplySettings()
    {
        _localOutlineMat.SetFloat(_OutlineSizePxID, _OutlineSizePx);
        _localOutlineMat.SetColor(outlineColorID, _outlineColor);
    }

   public void SetOutline(bool show)
    {
        _localOutlineMat.SetInt(OutlineOnID, System.Convert.ToInt32(show));
    }

    public void ChangeColor(Color color)
    {
        _localOutlineMat.SetColor(outlineColorID, color);
    }

    public void SetDefaultOutlineColor()
    {
        _localOutlineMat.SetColor(outlineColorID, _outlineColor);
    }
}
