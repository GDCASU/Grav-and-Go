using UnityEngine;

/* Author:
 * Steven Soto
 * 
 * Modified By:
 * 
 */

[System.Serializable]
public class Ammo
{
    public int Current { get; private set; }
    public int Max { get; private set; }

    public Ammo(int max)
    {
        Max = max;
        Current = max;
    }

    public bool Use(int amount = 1)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }

    public void Reload(int amount)
    {
        Current = Mathf.Clamp(Current + amount, 0, Max);
    }

    public void Refill()
    {
        Current = Max;
    }
}