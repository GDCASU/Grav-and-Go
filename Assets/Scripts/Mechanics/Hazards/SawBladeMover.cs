using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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
        Looping,
        PingPong
    }

    [Header("References")]
    [Tooltip("The object containing all the points of the sawblade path")]
    [SerializeField] private Transform pathPointContainer;
    [SerializeField] private Sawblade sawBlade;

    [Header("Motion Settings")]
    [Tooltip("Set the index of the node the object should start at")]
    [SerializeField] int initialPathPosition;
    [Tooltip("Interpolation curve the object follows between points")]
    [SerializeField] AnimationCurve interpolationCurve;
    [SerializeField] float travelSpeed;
    [Tooltip("How fast the sawblade spins (Cosmetic)")]
    [SerializeField] float rotationSpeed;
    [SerializeField] MotionType motion;

    [Header("Damage Settings")]
    [SerializeField] int damage;

    public int pathIndex = 0;
    private int direction = 1;
    private float t = 0f;

    private readonly List<Transform> pathPoints = new();

    private void Awake()
    {
        pathPoints.AddRange(pathPointContainer.GetComponentsInChildren<Transform>());

        if(motion == MotionType.PingPong)
        {
            direction *= -1;
        }
    }

    void Start()
    {
        pathIndex = initialPathPosition;
        sawBlade.transform.position = pathPoints[pathIndex].position;
        sawBlade.Init(damage);
    }

    private void Update()
    {
        sawBlade.transform.position = Vector3.MoveTowards(sawBlade.transform.position, pathPoints[pathIndex].position, travelSpeed * Time.deltaTime);
        sawBlade.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(sawBlade.transform.position, pathPoints[pathIndex].position) < 0.01f)
        {
            sawBlade.transform.position = pathPoints[pathIndex].position;
            
            switch(motion)
            {
                case MotionType.Looping:
                    pathIndex = (pathIndex + 1) % pathPoints.Count;
                    break;
                case MotionType.PingPong:
                    if(pathIndex == pathPoints.Count - 1 || pathIndex == 0)
                        direction *= -1;
                    pathIndex += direction;
                    break;
            };
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

        
    }

}
