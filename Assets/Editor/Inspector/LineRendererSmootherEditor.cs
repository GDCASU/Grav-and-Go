using System.Linq;
using UnityEditor;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 *
 * Modified By:
 *
 */// --------------------------------------------------------

// Modified from:
// https://github.com/llamacademy/line-renderer-bezier-path/tree/main

[CustomEditor(typeof(LineRendererSmoother))]
public class LineRendererSmootherEditor : Editor
{
    private LineRendererSmoother _smoother;
    private Vector3[]            _original;          // cached unsmoothed path

    private SerializedProperty _line;
    private SerializedProperty _length;
    private SerializedProperty _sections;

    void OnEnable()
    {
        _smoother = (LineRendererSmoother)target;
        if (_smoother.line == null) _smoother.line = _smoother.GetComponent<LineRenderer>();

        _line     = serializedObject.FindProperty("line");
        _length   = serializedObject.FindProperty("smoothingLength");
        _sections = serializedObject.FindProperty("smoothingSections");

        CacheOriginal();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_line);
        EditorGUILayout.PropertyField(_length);
        EditorGUILayout.PropertyField(_sections);
        EditorGUILayout.Space();

        GUI.enabled = _smoother.line != null && _smoother.line.positionCount >= 3;
        if (GUILayout.Button("Smooth Path (Editor)"))
        {
            Vector3[] raw = new Vector3[_smoother.line.positionCount];
            _smoother.line.GetPositions(raw);
            _smoother.ApplySmoothing(raw);
        }

        GUI.enabled = _original != null;
        if (GUILayout.Button("Restore Path"))
        {
            _smoother.line.positionCount = _original.Length;
            _smoother.line.SetPositions(_original);
        }

        if (GUILayout.Button("Refresh Original"))
        {
            CacheOriginal();
        }

        GUI.enabled = true;
        serializedObject.ApplyModifiedProperties();
    }

    private void CacheOriginal()
    {
        if (_smoother.line == null) return;
        _original = new Vector3[_smoother.line.positionCount];
        _smoother.line.GetPositions(_original);
    }
}