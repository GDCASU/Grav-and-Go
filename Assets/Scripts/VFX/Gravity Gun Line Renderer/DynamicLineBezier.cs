using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// Creates bezier lines on runtime given a list of transforms
/// </summary>
[RequireComponent(typeof(LineRendererSmoother))]
public class DynamicLineBezier : MonoBehaviour
{
    [Tooltip("First = player, last = target, anything between = bends.")]
    public List<Transform> controlPoints = new List<Transform>();

    private LineRendererSmoother smoother;

    void Awake() => smoother = GetComponent<LineRendererSmoother>();

    void Update()
    {
        if (controlPoints == null || controlPoints.Count < 2) return;

        // Gather live positions
        Vector3[] raw = new Vector3[controlPoints.Count];
        for (int i = 0; i < controlPoints.Count; i++) raw[i] = controlPoints[i].position;

        // One‑liner – build & apply
        smoother.ApplySmoothing(raw);
    }
}
