using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

/* -----------------------------------------------------------
 * Author:
 * Davyd Yehudin
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// All the logic related to laser pointers
/// </summary>
public class LaserPointer : MonoBehaviour
{
    const float laserTick = 0.1f;
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    [Header("Laser stuff")]
    private LineRenderer Laser;
    [SerializeField] public LaserColorEnum laserColor = LaserColorEnum.Red;
    [SerializeField] private float LaserWidth = 0.1f; //this is a random choice, change if needed
    [SerializeField] private GameObject Emitter = null;
    [SerializeField] private bool LaserOn = true;
    [Header("Laser Prefab to Spawn(Invisible)")]
    [SerializeField] public GameObject LaserPrefab;
    private Vector2 laserDir;
    private float LastFrameRotation = -1;
    private int vertexLimit = 40;
    private GameObject subLaser = null;
    private bool isSubLaser = false;

    void Awake()
    {
        LaserSetup();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(Emitter == null)
        {
            Debug.LogError("No emitter defined for laser " + this.name);
            return;
        }
        StartCoroutine(LaserCoroutine());
    }

    public void MakeThisSubLaser(float lifetime)
    {
        isSubLaser = true;
        Destroy(gameObject, lifetime);
    }

    //Because launching a bunch of raycasts every frame is not that good for performance
    //It is better to make it async and make it update less than every frame
    IEnumerator LaserCoroutine()
    {
        while (true)
        {
            LaserLoop();
            yield return new WaitForSeconds(laserTick);
        }
    }

    void LaserLoop()
    {
        if(!LaserOn)
        {
            Laser.positionCount = 0;
            return;
        }

        if(Laser == null)
            return;

        Color laserActualColor = global::LaserColor.getColorFromEnum(laserColor);
        Laser.startColor = laserActualColor;
        Laser.endColor = laserActualColor;

        if (this.transform.rotation.eulerAngles.z != LastFrameRotation)
        {
            LastFrameRotation = this.transform.rotation.eulerAngles.z;
            laserDir = CalculateRotation(LastFrameRotation);
        }

        CalculateWholeLaser();
    }

    //Calculate the direction vector given rotation using trigonometry
    Vector2 CalculateRotation(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad; //deg2rad is a const LMFAOOOOOOOOOO
        float y = Mathf.Sin(radians);
        float x = Mathf.Cos(radians);
        //if (_doDebugLog) Debug.Log("rotation " + x + " " + y);
        return new Vector2(x, y);
    }

    //Sets up a created laser.
    void LaserSetup()
    {
        // swap to GetComponenet?
        Laser = this.AddComponent<LineRenderer>();
        Laser.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")); // this finds a default line shader


        Color laserActualColor = global::LaserColor.getColorFromEnum(laserColor);
        Laser.startColor = laserActualColor;
        Laser.endColor = laserActualColor;
        
        if (_doDebugLog)
            Debug.Log(laserActualColor);

        LastFrameRotation = this.transform.rotation.eulerAngles.z;
        laserDir = CalculateRotation(LastFrameRotation);
        Laser.startWidth = LaserWidth;
        Laser.endWidth = LaserWidth;
        //Laser.positionCount = vertexLimit;
        
        //calculateWholeLaser();
    }
    

    void CalculateWholeLaser()
    {
        //if(subLaser != null) Destroy(subLaser);
        //List<Vector2> laser_positions;
        Laser.positionCount = vertexLimit;
        float Vert0x = Emitter.transform.position.x;
        float Vert0y = Emitter.transform.position.y;
        Laser.SetPosition(0, new Vector2(Vert0x, Vert0y));
        int reflectionCap = vertexLimit;
        int i = 0;
        Vector2 reflectVector = CalculateRotation(LastFrameRotation);
        while(i < reflectionCap - 1)
        {
            i++;

            //calculate the ith point of the laser using the (i-1)th point and an angle we calced before
            //calc origin to not hit the same object
            Vector2 posi = Laser.GetPosition(i - 1);
            Vector2 origin = posi + reflectVector * 0.01f;
            RaycastHit2D hit = Physics2D.Raycast(origin, reflectVector);
            if(hit.collider == null)
            {
                break;
            }
            Laser.SetPosition(i, hit.point);
            
            Vector2 norm = hit.normal;

            //check if we hit a mirror or a sensor or anything important in general
            GameObject hitObject = hit.transform.gameObject;
            if(hitObject.TryGetComponent(out LaserSensor sensor))
                sensor.LaserHit(laserColor);

            if(hitObject.TryGetComponent(out LaserCombiner sensorC))
                sensorC.OnLaserHit(laserColor, laserTick);

            //if we hit a portal we teleport
            if(hitObject.TryGetComponent(out PortalDefinition portal1))
            {
                GameObject portal2 = portal1.getExitPortal();
                //first find where we entered
                float enterX1 = hit.point.x;
                float enterY1 = hit.point.y;

                //now find where we would have entered for the second portal
                float enterX2 = portal2.transform.position.x + (enterX1 - hitObject.transform.position.x);
                float enterY2 = portal2.transform.position.y + (enterY1 - hitObject.transform.position.y);

                //we have the direction of the laser, now find the point where it would exit
                //all we need to do is to solve f(t) = entry + dir * t so that f(t) is outside the box
                //do this by finding bottom left and top right corners first
                Vector2 bottLeft = portal2.transform.position - portal2.transform.localScale/2;
                Vector2 topRight = portal2.transform.position + portal2.transform.localScale/2;

                //calculate how many times we need to use the dir vector to leave the portal
                float tx1 = (bottLeft.x - enterX2) / reflectVector.x;
                float tx2 = (topRight.x - enterX2) / reflectVector.x;
                float ty1 = (bottLeft.y - enterY2) / reflectVector.y;
                float ty2 = (topRight.y - enterY2) / reflectVector.y;

                //get the smaller of the big ones
                float tVector = Mathf.Min(Mathf.Max(tx1, tx2), Mathf.Max(ty1, ty2));
                Vector2 exit = new Vector2(enterX2, enterY2) + tVector * reflectVector;

                //because of how unity's lineRenderer works, we have to create a different laser
                //Quaternion reflection = Quaternion.LookRotation(reflectVector); //hopefully this is correct
                float angle = Mathf.Atan2(reflectVector.y, reflectVector.x) * Mathf.Rad2Deg; //convert vector into angle
                subLaser = Instantiate(LaserPrefab, exit, Quaternion.Euler(0,0,angle));
                LaserPointer  subLaserPointer = subLaser.GetComponent<LaserPointer>();
                subLaserPointer.laserColor = this.laserColor;
                subLaserPointer.MakeThisSubLaser(laserTick);
                subLaserPointer.LaserPrefab = this.LaserPrefab;

                //subLaser.
                //the subLaser should handle everything else on its own
                break;
            }
            if(!hitObject.TryGetComponent(out Mirror objectMirror)) break;
            if(!objectMirror.isMirror) break;
            
            
            //calc the reflectVector for the next iteration
            reflectVector = Vector2.Reflect(reflectVector, hit.normal).normalized;
            //if(_doDebugLog && i > 1) Debug.Log(i + " iteration, hit " + hitObject.name + " which is a mirror because " + objectMirror.isMirror + " x = " + reflectVector.x + " y = " + reflectVector.y);
        }
        Laser.positionCount = 1 + i;
    }

    public void SetLaserSwitch(bool sw) => LaserOn = sw;

}
