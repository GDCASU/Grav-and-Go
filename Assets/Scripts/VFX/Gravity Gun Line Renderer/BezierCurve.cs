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

[System.Serializable]
//  Lightweight utility representing a single cubic Bézier with convenience
//  methods to sample points along the curve.
public class BezierCurve
{
    // Cubic Bézier has exactly four control points (P0..P3):
    //   P0 = start position, P3 = end position, P1 & P2 = tangents.
    public Vector3[] Points;

    // Default constructor – allocates the 4‑element point array.
    public BezierCurve()
    {
        Points = new Vector3[4];
    }

    // Convenience constructor to create from an existing point array.
    public BezierCurve(Vector3[] Points)
    {
        this.Points = Points;
    }

    /// <summary>Read‑only property for the first control point (P0).</summary>
    public Vector3 StartPosition => Points[0];

    /// <summary>Read‑only property for the last control point (P3).</summary>
    public Vector3 EndPosition   => Points[3];

    // Curve evaluation
    // Equation from https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Cubic_B%C3%A9zier_curves
    // Given parameter t in [0,1] returns the position on the curve.
    public Vector3 GetSegment(float Time)
    {
        Time = Mathf.Clamp01(Time); // Safety clamp
        float time = 1 - Time;      // (1‑t)

        //       (1‑t)^3 · P0
        //   + 3(1‑t)^2 t · P1
        //   + 3(1‑t) t^2 · P2
        //   +      t^3 · P3
        return (time * time * time * Points[0])
             + (3 * time * time * Time * Points[1])
             + (3 * time * Time * Time * Points[2])
             + (Time * Time * Time * Points[3]);
    }

    /// <summary>
    /// Samples <paramref name="Subdivisions"/> evenly spaced points along the
    /// curve and returns them as an array.  Used by the editor when drawing
    /// a poly‑line approximation or writing back into the LineRenderer.
    /// </summary>
    public Vector3[] GetSegments(int Subdivisions)
    {
        Vector3[] segments = new Vector3[Subdivisions];

        for (int i = 0; i < Subdivisions; i++)
        {
            float time = (float)i / Subdivisions; // Normalised t parameter
            segments[i] = GetSegment(time);
        }

        return segments;
    }
}
