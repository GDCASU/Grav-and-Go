using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.TextCore.Text;

#if UNITY_EDITOR
using UnityEditor;

/// <summary> Sets a background color for game objects in the Hierarchy tab </summary>
[UnityEditor.InitializeOnLoad]
#endif
public class UnityHierarchyColoring
{
    private static Vector2 offset = new Vector2(20, 1);

    static UnityHierarchyColoring()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    static bool repaint = false;


    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        Color backgroundColor = Color.clear;
        Color textColor = Color.white;
        Color darkGrey = new Color(0.2f, 0.2f, 0.2f);
        Texture2D texture = null;

        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null)
        {
            Event e = Event.current;

            if (Selection.instanceIDs.Contains(instanceID))
            {
                repaint = false;
                textColor = Color.black;
                backgroundColor = new Color(0.2f, 0.4f, 0.6f);
            }

            /*
            else if (e.type == EventType.MouseDown && selectionRect.Contains(e.mousePosition))
            {
                repaint = true;
                textColor = Color.black;
                backgroundColor = new Color(0.2f, 0.4f, 0.6f);
            }
            */

            else if (!repaint)
            {
                switch (obj.name)
                {
                    case "Gameplay Camera":
                        backgroundColor = darkGrey;
                        textColor = new Color(0.4f, 0.7f, 1);
                        break;
                    case "-- Persistent Objects --":
                        backgroundColor = darkGrey;
                        textColor = Color.red;
                        break;
                    case "-- Environment --":
                        backgroundColor = darkGrey;
                        textColor = Color.green;
                        break;
                    case "-- Objects --":
                        backgroundColor = darkGrey;
                        textColor = Color.blue;
                        break;
                    case "-- Entities --":
                        backgroundColor = darkGrey;
                        textColor = Color.grey;
                        break;
                    case "-- VFXs --":
                        backgroundColor = darkGrey;
                        textColor = new Color(1, 0.47f, 0.7f);
                        break;
                }
            }

            if (backgroundColor != Color.clear)
            {
                Rect offsetRect = new Rect(selectionRect.position + offset, selectionRect.size);
                Rect bgRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width - 35, selectionRect.height);

                EditorGUI.DrawRect(selectionRect, backgroundColor);
                EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = textColor },
                    fontStyle = FontStyle.Bold
                }
                );

                if (texture != null)
                    EditorGUI.DrawPreviewTexture(new Rect(selectionRect.position, new Vector2(selectionRect.height, selectionRect.height)), texture);

                if (repaint) { EditorApplication.RepaintHierarchyWindow(); Debug.Log("HERE"); }
            }
        }
    }
}