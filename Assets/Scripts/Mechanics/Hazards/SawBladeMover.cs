using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;

/**
 * Matthew Glos 9/12/25
 * 
 * Sawblade path motion controller. Supports continous loop, disconnected loop, and ping pong style motion.
 * Supports an arbitrary number of points, customizable interpolation, and consistent motion speed across different path lengths.
 * 
 * could be used for moving other objects as well
 */
public class SawBladeMover : MonoBehaviour
{
    public enum motionType
    {
        Loop_Disconnected,
        Loop_Connected,
        PingPong
    }

    [Tooltip("List of Points the object travels between")]
    [SerializeField] List<Transform> PathPoints;

    [Tooltip("Automatically assign path points on start as the children of AutPathRoot. Heirarchy order determines the order the object traverses the points in.")]
    [SerializeField] bool AutoAssignPath;
    [SerializeField] GameObject AutoPathRoot;

    [Tooltip("Interpolation curve the object follows between points")]
    [SerializeField] AnimationCurve interpolationCurve;
    [SerializeField] float travelSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] motionType motion;

    [Tooltip("Set the index of the node the object should start at")]
    [SerializeField] int PathNodeInitialOffset;
    [Tooltip("Set how far along the first edge the object should start at as a percentage of the distance")]
    [SerializeField] float PathInitialOffsetPercent;


    private int pathIndex = 0;
    private int direction = 1;
    private float t = 0f;

    private Vector3 pos1;
    private Vector3 pos2;

    private bool returnTrip = false;
    void Start()
    {
        pathIndex = PathNodeInitialOffset;

        if (AutoAssignPath)
        {
            PathPoints = new List<Transform>();

            foreach (Transform g in AutoPathRoot.GetComponentsInChildren<Transform>())
            {
                if (g != AutoPathRoot.transform)
                    PathPoints.Add(g);
            }
        }

        pos1 = PathPoints[pathIndex].position;
        pos2 = PathPoints[pathIndex + 1].position;

        this.transform.position = Vector3.Lerp(pos1, pos2, interpolationCurve.Evaluate(PathInitialOffsetPercent));
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        t += Time.deltaTime * travelSpeed;
        float animIndex = ((t / (Vector3.Distance(pos1, pos2))) + PathInitialOffsetPercent) % 1f;
        this.transform.position = Vector3.Lerp(pos1, pos2, interpolationCurve.Evaluate(animIndex));

        if (Vector3.Distance(this.transform.position, pos2) > .01f) return;

        PathInitialOffsetPercent = 0f;
        switch (motion)
        {
            case motionType.Loop_Connected:
                {
                    pathIndex += 1;
                    t = 0;

                    pos1 = PathPoints[pathIndex % PathPoints.Count].position;
                    pos2 = PathPoints[(pathIndex + 1) % PathPoints.Count].position;

                    break;
                }

            case motionType.Loop_Disconnected:
                {
                    pathIndex += 1;
                    t = 0;

                    if (pathIndex >= PathPoints.Count - 1)
                    {
                        pathIndex = 0;
                        pos1 = PathPoints[pathIndex % PathPoints.Count].position;
                        pos2 = PathPoints[(pathIndex + 1) % PathPoints.Count].position;
                        transform.position = pos1;
                        break;
                    }

                    pos1 = PathPoints[pathIndex % PathPoints.Count].position;
                    pos2 = PathPoints[(pathIndex + 1) % PathPoints.Count].position;

                    break;
                }

            case motionType.PingPong:
                {
                    pathIndex += direction;
                    t = 0;

                    if (pathIndex >= PathPoints.Count - 1 || pathIndex == 0)
                    {
                        direction *= -1;
                    }

                    pos1 = PathPoints[pathIndex % PathPoints.Count].position;
                    pos2 = PathPoints[(pathIndex + direction) % PathPoints.Count].position;

                    break;
                }
        }
    }
    private void OnDrawGizmos()
    {
        if (PathPoints == null || PathPoints.Count < 2)
            return;

        Gizmos.color = Color.red;

        for (int i = 0; i < PathPoints.Count - 1; i++)
        {
            if (PathPoints[i] != null && PathPoints[i + 1] != null)
            {
                Gizmos.DrawLine(PathPoints[i].position, PathPoints[i + 1].position);
            }
        }

        if (motion == motionType.Loop_Connected && PathPoints.First() != null && PathPoints.Last() != null)
        {
            Gizmos.DrawLine(PathPoints.Last().position, PathPoints.First().position);
        }
    }

}