using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Davyd Yehudin
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// the definition of the portal class
/// </summary>
public class PortalDefinition : MonoBehaviour
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

    [Header("Who goes through the portal")]
    [SerializeField] private bool LetPlayerThrough = true;
    [SerializeField] private bool LetObjectsThrough = true;

    [Header("Portal's exit and on state")]
    [SerializeField] private GameObject exitPortal = null;
    [SerializeField] private bool portalOnState = true;
    private PortalDefinition exitPortalScript;


    private void Awake()
    {
        if (exitPortal == null)
        {
            Debug.LogError("No exit portal defined for a portal, portal name: " + this.name);
            return;
        }
        if(!exitPortal.TryGetComponent<PortalDefinition>(out exitPortalScript))
        {
            Debug.LogError("Portal " + this.name + " is connected to " + exitPortal.name + " which does not have the Portal script");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exitPortal == null)
        {
            Debug.LogError("No exit portal defined for a portal, portal name: " + this.name);
            return;
        }
        if (!portalOnState)
        {
            return;
        }
        if (other.tag == "Player" && LetPlayerThrough)
        {
            teleport(other.transform);
        }
        if (other.tag == "Physics Object" && LetObjectsThrough)
        {
            teleport(other.transform);
        }
        Debug.Log(other.tag);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
    }

    //teleport function to teleport things and do spaghetti

    private void teleport(Transform otherTransform)
    {
        exitPortalScript.setOnState(false);
        otherTransform.position = exitPortal.transform.position;
        StartCoroutine(turnPortalBackOnIn6Frames());
    }

    //pretty self explanatory, also this is only true for 60 fps but whatever
    private IEnumerator turnPortalBackOnIn6Frames()
    {
        yield return new WaitForSeconds(0.1f);
        exitPortalScript.setOnState(true);
        yield break;
    }

    //getters and setters for some private variables

    public GameObject getExitPortal()
    {
        return exitPortal;
    }

    public void setExitPortal(GameObject newExitPortal)
    {
        exitPortal = newExitPortal;
    }

    public bool getOnState()
    {
        return portalOnState;
    }

    public void setOnState(bool newState)
    {
        portalOnState = newState;
    }
}
