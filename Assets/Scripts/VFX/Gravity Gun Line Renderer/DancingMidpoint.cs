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
/// Drifts inside a sphere of <paramref name="radius"/> at a *constant* speed.
/// The point picks a random destination within the radius, moves toward it
/// </summary>
public class DancingMidpoint : MonoBehaviour
{
    [Tooltip("Maximum distance from the centre (units).")]
    public float radius = 1f;

    [Tooltip("Movement speed in units per second.")]
    public float speed = 0.2f;

    [Tooltip("Use local coordinates (true) or world space (false).")]
    public bool useLocalSpace = true;

    private Vector3 centre;       // fixed centre position
    private Vector3 targetOffset; // current destination offset

    void Awake()
    {
        centre = useLocalSpace ? transform.localPosition : transform.position;
        PickNewTarget();
    }

    void Update()
    {
        Vector3 current = useLocalSpace ? transform.localPosition : transform.position;
        Vector3 target  = centre + targetOffset;

        // MoveToward at constant speed
        Vector3 next = Vector3.MoveTowards(current, target, speed * Time.deltaTime);
        ApplyPosition(next);

        // Arrived?  Pick a fresh destination.
        if ((next - target).sqrMagnitude < 0.0001f)
            PickNewTarget();
    }

    // helpers 

    private void PickNewTarget()
    {
        targetOffset = Random.insideUnitSphere * radius;
    }

    private void ApplyPosition(Vector3 pos)
    {
        if (useLocalSpace)
            transform.localPosition = pos;
        else
            transform.position = pos;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 centreWorld;
        if (Application.isPlaying)
        {
            centreWorld = useLocalSpace ? (transform.parent ? transform.parent.TransformPoint(centre) : centre) : centre;
        }
        else
        {
            centreWorld = useLocalSpace ? (transform.parent ? transform.parent.TransformPoint(transform.localPosition) : transform.position) : transform.position;
        }

        Gizmos.DrawWireSphere(centreWorld, radius);
    }
}
