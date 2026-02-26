using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
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
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    [Header("Laser stuff")]
    private LineRenderer Laser;
    [SerializeField] private LaserColorEnum laserColor = LaserColorEnum.Red;
    [SerializeField] private float LaserWidth = 0.1f; //this is a random choice, change if needed
    [SerializeField] private GameObject Emitter = null;
    [SerializeField] private bool LaserOn = true;
    private Vector2 laserDir;
    private float LastFrameRotation = -1;
    private int vertexLimit = 40;
    // Start is called before the first frame update
    void Start()
    {
        Laser = this.AddComponent<LineRenderer>();
        if(Emitter == null)
        {
            Debug.LogError("No emitter defined for laser " + this.name);
            return;
        }
        LaserSetup();
        StartCoroutine(LaserCoroutine());
    }

    // Update is called once per frame
    /*void Update()
    {
        if(!LaserOn) return;
        Color laserActualColor = LaserColor.getColorFromEnum(laserColor);
        Laser.startColor = laserActualColor;
        Laser.endColor = laserActualColor;
        if (this.transform.rotation.eulerAngles.z != LastFrameRotation)
        {
        LastFrameRotation = this.transform.rotation.eulerAngles.z;
        laserDir = CalculateRotation(LastFrameRotation);
        }
        float Vert0x = Emitter.transform.position.x;
        float Vert0y = Emitter.transform.position.y;
        //Laser.SetPosition(0, new Vector2(Vert0x, Vert0y));
        calculateWholeLaser();
        //Laser.SetPosition(1, calcVertex());
    }*/

    //Because launching a bunch of raycasts every frame is not that good for performance
    //It is better to make it async and make it update less than every frame
    IEnumerator LaserCoroutine()
    {
        while (true)
        {
            LaserLoop();
            yield return new WaitForSeconds(0.1f);
        }
    }

    void LaserLoop()
    {
        if(!LaserOn) return;
        Color laserActualColor = LaserColor.getColorFromEnum(laserColor);
        Laser.startColor = laserActualColor;
        Laser.endColor = laserActualColor;
        if (this.transform.rotation.eulerAngles.z != LastFrameRotation)
        {
            LastFrameRotation = this.transform.rotation.eulerAngles.z;
            laserDir = CalculateRotation(LastFrameRotation);
        }
        float Vert0x = Emitter.transform.position.x;
        float Vert0y = Emitter.transform.position.y;
        //Laser.SetPosition(0, new Vector2(Vert0x, Vert0y));
        calculateWholeLaser();
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
        if (Laser == null)
        {
            return;
        }
        Laser.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")); // this finds a default line shader
        //also colors don't work without the line above XD


        Color laserActualColor = LaserColor.getColorFromEnum(laserColor);
        Laser.startColor = laserActualColor;
        Laser.endColor = laserActualColor;
        if (_doDebugLog)
        {
            Debug.Log(laserActualColor);
        }
        LastFrameRotation = this.transform.rotation.eulerAngles.z;
        laserDir = CalculateRotation(LastFrameRotation);
        Laser.startWidth = LaserWidth;
        Laser.endWidth = LaserWidth;
        //Laser.positionCount = vertexLimit;
        
        //calculateWholeLaser();
    }
    

    void calculateWholeLaser()
    {
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

            //check if we hit a mirror or a sensor
            GameObject hitObject = hit.transform.gameObject;
            if(hitObject.TryGetComponent(out LaserSensor sensor))
            {
                sensor.LaserHit(laserColor);
                break;
            }
            if(hitObject.TryGetComponent(out ColorSensor sensorC))
            {
                sensorC.laserHit(laserColor);
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

    //some setters and getters
    //if there isn't one you need just make it I am lazy

    public void setLaserSwitch(bool sw)
    {
        LaserOn = sw;
    }

    public void setLaserColor(LaserColorEnum newCol)
    {
        laserColor = newCol;
    }

    public LaserColorEnum getLaserColor()
    {
        return laserColor;
    }
}
