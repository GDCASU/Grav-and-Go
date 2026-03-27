using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Davyd Yehudin
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// An enum for laser colors. R is 2, G is 3, B is 5. Combinations are just multiplications
/// </summary>
public enum LaserColorEnum
{
    None = 1,
    Red = 2,
    Lime = 3,
    Cyan = 5,
    Orange = 6,
    Pink = 10,
    Purple = 15,
    White = 30
}

class LaserColor
{
    //please don't comprehend what is written below
    public static Color getColorFromEnum(LaserColorEnum a)
    {
        switch (a)
        {
            case LaserColorEnum.Red:
                return new Color(247f/255f, 22f/255f, 47f/255f, 1f);
            case LaserColorEnum.Lime:
                return new Color(150f/255f, 1f, 65f/255f, 1f);
            case LaserColorEnum.Cyan:
                return new Color(74f/255f, 214f/255f, 240f/255f, 1f);
            case LaserColorEnum.Orange:
                return new Color(1f, 118f/255f, 20f/255f, 1f);
            case LaserColorEnum.Pink:
                return new Color(250f/255f, 42f/255f, 176f/255f, 1f);
            case LaserColorEnum.Purple:
                return new Color(173f/255f, 53f/255f, 1f, 1f);
            default:
                //gaster
                return Color.white;
        }
    }
}
