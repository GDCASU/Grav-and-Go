using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 *
 * Modified By:
 *
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Purpose:
 * – draws a compass arrow + Vector2 field on one line
 * – respects [InspectorReadOnly] so the field becomes non‑editable
 */// --------------------------------------------------------

[CustomPropertyDrawer(typeof(Vector2CompassAttribute))]
public class Vector2CompassDrawer : PropertyDrawer
{
    private const float CompassSize = 70f;  // px
    private const float ArrowThickness = 2f;
    private const float Gap = 6f;   // between field & compass
    private const float FieldWidth = 110f; // Vector2Field width

    // helper: is the field also tagged [InspectorReadOnly]?
    private bool IsReadOnly() =>
        fieldInfo.GetCustomAttribute(typeof(InspectorReadOnly), inherit: true) != null;

    // height: one line (whichever is taller)
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) =>
        Mathf.Max(CompassSize, EditorGUIUtility.singleLineHeight) + 2f;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        bool readOnly = IsReadOnly();

        EditorGUI.BeginProperty(pos, label, prop);

        // layout maths
        float lineH  = EditorGUIUtility.singleLineHeight;
        float labelW = EditorGUIUtility.labelWidth; // Unity's label column

        // label rect (vertically centred)
        var labelRect = new Rect(pos.x,
                                 pos.y + (CompassSize - lineH) * 0.5f,
                                 labelW,
                                 lineH);

        // strip to the right of the label
        float stripW  = pos.width - labelW;
        float groupW  = FieldWidth + Gap + CompassSize;
        float groupX  = pos.x + labelW + (stripW - groupW) * 0.5f;

        // Vector2 field rect
        var fieldRect = new Rect(groupX,
                                 labelRect.y,
                                 FieldWidth,
                                 lineH);

        // compass rect
        var compassRect = new Rect(groupX + FieldWidth + Gap,
                                   pos.y,
                                   CompassSize,
                                   CompassSize);

        // draw label
        EditorGUI.LabelField(labelRect, label);

        // numeric Vector2 field (disabled if read‑only)
        using (new EditorGUI.DisabledGroupScope(readOnly))
        {
            prop.vector2Value =
                EditorGUI.Vector2Field(fieldRect, GUIContent.none, prop.vector2Value);
        }

        // compass arrow
        DrawCompass(compassRect, prop.vector2Value);

        EditorGUI.EndProperty();
    }
    
    /// <summary>
    /// Compass Gizmo
    /// </summary>
    private void DrawCompass(Rect rect, Vector2 v)
    {
        var center = rect.center;
        float radius = rect.width * 0.5f - 4f;

        Handles.BeginGUI();

        // circle
        Handles.color = Color.gray;
        Handles.DrawWireDisc(center, Vector3.forward, radius);

        // arrow (flip Y for GUI coords)
        Vector2 dir = v;
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        Vector2 guiDir = new Vector2(dir.x, -dir.y);
        Vector2 tip    = center + guiDir * radius;

        Handles.color = Color.yellow;
        Handles.DrawAAPolyLine(ArrowThickness, center, tip);

        // arrow head
        const float head = 6f;
        Vector2 perp  = new Vector2(-guiDir.y, guiDir.x);
        Vector2 left  = tip - guiDir * head + perp * head * 0.5f;
        Vector2 right = tip - guiDir * head - perp * head * 0.5f;
        Handles.DrawAAConvexPolygon(tip, left, right);

        Handles.EndGUI();
    }
}