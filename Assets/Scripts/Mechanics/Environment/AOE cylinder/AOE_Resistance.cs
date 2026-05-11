using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Joshua Wright
 * 
 * Modified By: 
 * 
 * 
 */// --------------------------------------------------------
public class AOE_Resistance : MonoBehaviour
{
    [Header("Resistance Settings")]
    [Tooltip("0.0 = full damage/effect, 1.0 = total immunity")]
    [Range(0f, 2f)] // Increased range in case you want "weakness" (factors > 1)
    public float resistanceFactor = 0f;

    /// <summary>
    /// Converts resistance (0 to 1) into a force multiplier.
    /// 0 resistance -> 1.0 multiplier
    /// 1 resistance -> 0.0 multiplier
    /// </summary>
    public float GetForceMultiplier()
    {
        // Clamp at 0 so high resistance doesn't reverse the force direction
        return Mathf.Max(0, 1f - resistanceFactor);
    }

    public bool IsFullyResistant() => resistanceFactor >= 1f;
}