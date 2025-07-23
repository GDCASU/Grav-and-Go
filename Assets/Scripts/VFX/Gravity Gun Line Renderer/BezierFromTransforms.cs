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
/// Feeds the current positions of a list of Transforms into a
/// LineRendererSmoother each frame
/// </summary>
[RequireComponent(typeof(LineRendererSmoother))]
public class BezierFromTransforms : MonoBehaviour
{
    [Tooltip("First point is the player, last point is the target. " +
             "Anything in between becomes a bend in the beam.")]
    public List<Transform> points = new List<Transform>();

    private LineRendererSmoother smoother;

    void Awake()
    {
        smoother = GetComponent<LineRendererSmoother>();
    }

    void Update()
    {
        // Quick sanity check
        if (points == null || points.Count < 2) return;

        // Bake the current Transform positions into a raw poly‑line
        Vector3[] raw = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
            raw[i] = points[i].position;
    }
}
