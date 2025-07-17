using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
[CustomPropertyDrawer(typeof(Vector2CompassAttribute))]
public class Vector2CompassDrawer : PropertyDrawer
{
    const float CompassSize    = 70f; // diameter in px
    const float ArrowThickness = 2f;

    /* -------- Height: gizmo + numeric field -------- */
    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
{
    // One‑line height = the taller of the compass or the standard line
    return Mathf.Max(CompassSize, EditorGUIUtility.singleLineHeight) + 2f;
}

public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
{
    EditorGUI.BeginProperty(pos, label, prop);

    /* ───────────────────────── LAYOUT ───────────────────────── */

    float lineH      = EditorGUIUtility.singleLineHeight;
    float labelW     = EditorGUIUtility.labelWidth;      // Unity’s standard label width
    const float gap  = 6f;                               // space between field & compass
    const float fldW = 110f;                             // fixed width for the Vector2Field

    // label rect – vertically centred in the row
    var labelRect = new Rect(pos.x,
                             pos.y + (CompassSize - lineH) * 0.5f,
                             labelW,
                             lineH);

    // remaining strip to the right of the label
    float rightStripW = pos.width - labelW;

    // total width of the "field + compass" group
    float groupW = fldW + gap + CompassSize;

    // left edge of the group so it’s centred in the strip
    float groupX = pos.x + labelW + (rightStripW - groupW) * 0.5f;

    // Vector2Field rect
    var fieldRect = new Rect(groupX,
                             labelRect.y,
                             fldW,
                             lineH);

    // compass rect (flush to the right of the field)
    var compassRect = new Rect(groupX + fldW + gap,
                               pos.y,
                               CompassSize,
                               CompassSize);

    /* ───────────────────────── DRAW ───────────────────────── */

    // label
    EditorGUI.LabelField(labelRect, label);

    // Vector2 field
    prop.vector2Value = EditorGUI.Vector2Field(fieldRect,
                                               GUIContent.none,
                                               prop.vector2Value);

    // compass
    DrawCompass(compassRect, prop.vector2Value);

    EditorGUI.EndProperty();
}

    /* =========================================================== */
    /*                         GIZMO DRAW                          */
    /* =========================================================== */
    void DrawCompass(Rect rect, Vector2 v)
    {
        var center = rect.center;
        float radius = rect.width * 0.5f - 4f;  // padding

        Handles.BeginGUI();
        {
            /* circle */
            Handles.color = Color.gray;
            Handles.DrawWireDisc(center, Vector3.forward, radius);

            /* arrow (flip Y for GUI coords) */
            Vector2 dir = v;
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            Vector2 guiDir = new Vector2(dir.x, -dir.y);          // invert Y
            Vector2 tip    = center + guiDir * radius;

            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(ArrowThickness, center, tip);

            /* arrow head */
            const float head  = 6f;
            Vector2 perp = new Vector2(-guiDir.y, guiDir.x);
            Vector2 left  = tip - guiDir * head + perp * head * 0.5f;
            Vector2 right = tip - guiDir * head - perp * head * 0.5f;
            Handles.DrawAAConvexPolygon(tip, left, right);
        }
        Handles.EndGUI();
    }
}
