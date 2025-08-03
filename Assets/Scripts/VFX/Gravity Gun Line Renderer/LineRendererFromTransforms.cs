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
/// Copies the positions of a list of Transforms into a LineRenderer each Update.
/// Handy for “draw a line through these objects” tasks.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LineRendererFromTransforms : MonoBehaviour
{
    [Tooltip("The points the line should pass through, in order.")]
    public List<Transform> points = new List<Transform>();

    private LineRenderer line;
    private Vector3[]    positionsCache;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (points == null || points.Count == 0) return;

        // Grow the cache if needed (avoids new[] every frame)
        if (positionsCache == null || positionsCache.Length != points.Count)
            positionsCache = new Vector3[points.Count];

        // Copy positions
        for (int i = 0; i < points.Count; i++)
            positionsCache[i] = points[i] ? points[i].position : Vector3.zero;

        // Push to LineRenderer
        line.positionCount = positionsCache.Length;
        line.SetPositions(positionsCache);
    }
}