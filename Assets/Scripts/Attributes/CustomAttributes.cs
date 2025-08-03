using System;
using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Purpose:
 * Hold a list of custom editor attributes to make programming the
 * game easier
 */// --------------------------------------------------------


/// <summary>
/// Creates a [InspectorReadOnly] Attribute so we can expose values
/// to the inspector while not allowing its editting.
/// </summary>
public class InspectorReadOnly : PropertyAttribute { }

/// <summary>
/// Creates a [Vector2Compass] Attribute to draw vector2s on the gui
/// </summary>
public class Vector2CompassAttribute : PropertyAttribute { }

/// <summary>
/// Creates a [RightAlignToggle] attribute that will render bool checkboxes to the right in the inspector
/// </summary>
public class RightAlignToggleAttribute : PropertyAttribute { }

/// <summary>
/// Creates a [InlineToggle] attribute that places the bool checkbox right after the text, without blocking it
/// </summary>
public class InlineToggleAttribute : PropertyAttribute { }

// StringInList and PropertyDrawerHelper obtained from gist
// https://gist.github.com/ProGM/9cb9ae1f7c8c2a4bd3873e4df14a6687


/// <summary>
/// Creates an attribute [StringInList("A", "B")] as to create a dropdown that isnt prone to typing errors
/// </summary>
public class StringInList : PropertyAttribute
{
    public delegate string[] GetStringList();

    public StringInList(params string[] list)
    {
        List = list;
    }

    public StringInList(Type type, string methodName)
    {
        var method = type.GetMethod(methodName);
        if (method != null)
        {
            List = method.Invoke(null, null) as string[];
        }
        else
        {
            Debug.LogError("NO SUCH METHOD " + methodName + " FOR " + type); 
        }
    }

    public string[] List { get; private set; }
}

/// <summary>
/// Helper drawer to make a special string dropdown that has all scenes in the build
/// </summary>
public static class PropertyDrawersHelper
{
#if UNITY_EDITOR

    public static string[] AllSceneNames()
    {
        var temp = new List<string>();
        foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
        {
            if (S.enabled)
            {
                string name = S.path.Substring(S.path.LastIndexOf('/') + 1);
                name = name.Substring(0, name.Length - 6);
                temp.Add(name);
            }
        }

        return temp.ToArray();
    }

#endif
}