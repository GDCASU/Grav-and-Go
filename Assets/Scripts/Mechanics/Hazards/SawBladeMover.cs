using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/**
 * Matthew Glos 9/12/25
 * 
 * Sawblade path motion controller. Supports continous loop, disconnected loop, and ping pong style motion.
 * Supports an arbitrary number of points, customizable interpolation, and consistent motion speed across different path lengths.
 * 
 * could be used for moving other objects as well
 * 
 * Modified By:
 * Chandler Van
 */
public class SawBladeMover : MonoBehaviour
{
    public enum MotionType
    {
        Loop_Disconnected,
        Loop_Connected,
        PingPong
    }

    [Header("Path Settings")]
    [Tooltip("The object containing all the points of the sawblade path")]
    [SerializeField] private Transform pathPointContainer;
    [Tooltip("Set the index of the node the object should start at")]
    [SerializeField] int pathNodeInitialIndex;

    [Header("Motion Settings")]
    [Tooltip("Interpolation curve the object follows between points")]
    [SerializeField] AnimationCurve interpolationCurve;
    [SerializeField] float travelSpeed;
    [Tooltip("How fast the sawblade spins (Cosmetic)")]
    [SerializeField] float rotationSpeed;
    [SerializeField] MotionType motion;

    private int pathIndex = 0;
    private int direction = 1;
    private float t = 0f;

    private Vector3 pos1;
    private Vector3 pos2;

    private readonly List<Transform> pathPoints;

    private void Awake()
    {
        for(int i = 0; i < pathPointContainer.childCount; i++)
            pathPoints.Add(pathPointContainer.GetChild(i));
    }

    void Start()
    {
        pathIndex = pathNodeInitialIndex;

        pos1 = pathPoints[pathIndex].position;
        pos2 = pathPoints[pathIndex + 1].position;

        this.transform.position = pos1;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        t += Time.deltaTime * travelSpeed;
        float animIndex = ((t / (Vector3.Distance(pos1, pos2)))) % 1f;
        this.transform.position = Vector3.Lerp(pos1, pos2, interpolationCurve.Evaluate(animIndex));

        if (Vector3.Distance(this.transform.position, pos2) > .01f) return;

        switch (motion)
        {
            case MotionType.Loop_Connected:
                {
                    pathIndex += 1;
                    t = 0;

                    pos1 = pathPoints[pathIndex % pathPoints.Count].position;
                    pos2 = pathPoints[(pathIndex + 1) % pathPoints.Count].position;

                    break;
                }

            case MotionType.Loop_Disconnected:
                {
                    pathIndex += 1;
                    t = 0;

                    if (pathIndex >= pathPoints.Count - 1)
                    {
                        pathIndex = 0;
                        pos1 = pathPoints[pathIndex % pathPoints.Count].position;
                        pos2 = pathPoints[(pathIndex + 1) % pathPoints.Count].position;
                        transform.position = pos1;
                        break;
                    }

                    pos1 = pathPoints[pathIndex % pathPoints.Count].position;
                    pos2 = pathPoints[(pathIndex + 1) % pathPoints.Count].position;

                    break;
                }

            case MotionType.PingPong:
                {
                    pathIndex += direction;
                    t = 0;

                    if (pathIndex >= pathPoints.Count - 1 || pathIndex == 0)
                    {
                        direction *= -1;
                    }

                    pos1 = pathPoints[pathIndex % pathPoints.Count].position;
                    pos2 = pathPoints[(pathIndex + direction) % pathPoints.Count].position;

                    break;
                }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(1);
        }
    }

    private void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Count < 2)
            return;

        Gizmos.color = Color.red;

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            if (pathPoints[i] != null && pathPoints[i + 1] != null)
            {
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
            }
        }

        if (motion == MotionType.Loop_Connected && pathPoints.First() != null && pathPoints.Last() != null)
        {
            Gizmos.DrawLine(pathPoints.Last().position, pathPoints.First().position);
        }
    }

}
