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
/// Re‑positions a list of “mid” points so they stay perfectly
/// equidistant along the line from start -> end.
/// </summary>
public class EquidistantPointsUpdater : MonoBehaviour
{
    [Tooltip("Transform at the beginning of the beam (player).")]
    public Transform start;

    [Tooltip("Transform at the end of the beam (target).")]
    public Transform end;

    [Tooltip("All middle points, in order from start to end.")]
    public List<Transform> points = new List<Transform>();

    void Update()
    {
        // Bail out if anything is missing
        if (!start || !end || points.Count == 0) return;

        Vector3 direction = end.position - start.position;
        float segment     = direction.magnitude / (points.Count + 1);
        Vector3 step      = direction.normalized * segment;

        for (int i = 0; i < points.Count; i++)
        {
            if (!points[i]) continue;      // skip gaps in the list
            points[i].position = start.position + step * (i + 1);
        }
    }
}
