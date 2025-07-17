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