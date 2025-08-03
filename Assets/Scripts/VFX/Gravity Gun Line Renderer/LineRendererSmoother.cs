using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 *
 * Modified By:
 *
 */// --------------------------------------------------------

// Modified from:
// https://github.com/llamacademy/line-renderer-bezier-path/tree/main

[RequireComponent(typeof(LineRenderer))]
public class LineRendererSmoother : MonoBehaviour
{
    [Tooltip("The LineRenderer that will be smoothed.")]
    public LineRenderer line;

    [Tooltip("Tangential strength of the curve.")]
    public float smoothingLength = 2f;

    [Tooltip("Straight‑line subdivisions *per Bézier segment*. Higher = smoother.")]
    public int smoothingSections = 10;

    void Reset() { line = GetComponent<LineRenderer>(); }

    // Public one‑liner API
    public void ApplySmoothing(Vector3[] rawPath)
    {
        if (rawPath == null || rawPath.Length < 3) return;
        List<Vector3> smooth = GenerateSmoothPath(rawPath, smoothingLength, smoothingSections);
        line.positionCount = smooth.Count;
        line.SetPositions(smooth.ToArray());
    }

    // Core algorithm
    private static List<Vector3> GenerateSmoothPath(Vector3[] raw, float length, int sections)
    {
        List<Vector3> outVerts = new List<Vector3>((raw.Length - 1) * sections + 1);

        // Ian: Math for days lol, go look up wikipedia bezier lines for a better explanation,
        // Cuz I aint doing it here
        for (int i = 0; i < raw.Length - 1; i++)
        {
            Vector3 p0 = raw[i];
            Vector3 p3 = raw[i + 1];

            Vector3 prevDir = (i == 0) ? (p3 - p0).normalized : (p0 - raw[i - 1]).normalized;
            Vector3 nextDir = (i == raw.Length - 2) ? (p3 - p0).normalized : (raw[i + 2] - p3).normalized;
            Vector3 curDir  = (p3 - p0).normalized;

            Vector3 p1 = p0 + (prevDir + curDir) * length;
            Vector3 p2 = p3 + (nextDir + curDir) * -length;

            if (i == 0 && raw.Length >= 3)
            {
                Vector3 seg1 = (raw[1] - raw[0]).normalized;
                Vector3 seg2 = (raw[2] - raw[1]).normalized;
                p2 = raw[1] + (seg2 + seg1) * -length;
            }

            BezierCurve curve = new BezierCurve(new Vector3[] { p0, p1, p2, p3 });
            for (int j = 0; j <= sections; j++)
            {
                if (i > 0 && j == 0) continue; // skip duplicate at segment joins
                float t = (float)j / sections;
                outVerts.Add(curve.GetSegment(t));
            }
        }

        return outVerts;
    }
}