using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Attribute that will make bool boxes align with the right instead of being in the middle
/// </summary>
[CustomPropertyDrawer(typeof(RightAlignToggleAttribute))]
public class RightAlignToggleDrawer : PropertyDrawer
{
    private const float ToggleSize = 18f;      // default Unity toggle size

    /* ---------- helper: is this field also read‑only? ---------- */
    private bool IsReadOnly() =>
        fieldInfo.GetCustomAttribute(typeof(InspectorReadOnly), inherit: true) != null;

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) =>
        EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        if (prop.propertyType != SerializedPropertyType.Boolean)
        {
            EditorGUI.LabelField(pos, label.text, "Use RightAlignToggle on bools only");
            return;
        }

        bool readOnly = IsReadOnly();

        /* ── layout: label left, toggle flush right ───────────── */
        float toggleX = pos.x + pos.width - ToggleSize;
        var   labelRect  = new Rect(pos.x, pos.y, pos.width - ToggleSize, pos.height);
        var   toggleRect = new Rect(toggleX, pos.y, ToggleSize, pos.height);

        EditorGUI.BeginProperty(pos, label, prop);

        // label
        EditorGUI.LabelField(labelRect, label);

        // toggle (disabled if read‑only)
        using (new EditorGUI.DisabledGroupScope(readOnly))
        {
            bool newVal = EditorGUI.Toggle(toggleRect, GUIContent.none, prop.boolValue);
            if (!readOnly && newVal != prop.boolValue)
                prop.boolValue = newVal;
        }

        EditorGUI.EndProperty();
    }
}
