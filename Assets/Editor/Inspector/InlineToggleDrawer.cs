using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Class that draws the bool checkbox inlined on the inspector
/// </summary>
[CustomPropertyDrawer(typeof(InlineToggleAttribute))]
public class InlineToggleDrawer : PropertyDrawer
{
    private const float ToggleSize = 18f;
    private const float Gap        = 4f;

    private bool IsReadOnly() =>
        fieldInfo.GetCustomAttribute(typeof(InspectorReadOnly), inherit: true) != null;

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) =>
        EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        if (prop.propertyType != SerializedPropertyType.Boolean)
        {
            EditorGUI.LabelField(pos, label.text, "InlineToggle works with bools only");
            return;
        }

        bool readOnly = IsReadOnly();

        /* measure label width so toggle starts right after text */
        float labelW = EditorStyles.label.CalcSize(label).x;
        labelW = Mathf.Min(labelW, pos.width - ToggleSize - Gap);

        var labelRect  = new Rect(pos.x, pos.y, labelW, pos.height);
        var toggleRect = new Rect(labelRect.xMax + Gap, pos.y, ToggleSize, pos.height);

        EditorGUI.BeginProperty(pos, label, prop);

        EditorGUI.LabelField(labelRect, label);

        using (new EditorGUI.DisabledGroupScope(readOnly))
        {
            bool newVal = EditorGUI.Toggle(toggleRect, GUIContent.none, prop.boolValue);
            if (!readOnly && newVal != prop.boolValue)
                prop.boolValue = newVal;
        }

        EditorGUI.EndProperty();
    }
}
