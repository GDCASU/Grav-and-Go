using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Davyd Yehudin
 * 
 * Modified By: Justin Miller
 * Joshua Wright
 * 
 */// --------------------------------------------------------

/// <summary>
/// calls an effect from the EffectList script using an enum when an object goes into the collider.
/// Add this to any object with collider2D with isTrigger on (example in this directory)
/// </summary>

 public class AOE_cylinder : MonoBehaviour
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Customization")]
    [SerializeField] private float thrust = 100f;
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;
    [SerializeField] public bool _isInCylinder;
    [SerializeField] private effectsEnum effectNumber = 0;
    [SerializeField] private GameObject[] effectParticles;
    private GameObject currentParticle = null;
    private effectsEnum lastEffectNumber;




    //call setEffect to create particle effects based on effectNumber
    void Start()
    {
        setEffect(effectNumber);
    }
    
   //check to see if the effectNumber changes mid gameplay if so call setEffect with the new effectNumber
    
    void Update()
    {
        if (effectNumber != lastEffectNumber)
        {
            setEffect(effectNumber);
            lastEffectNumber = effectNumber;
        }
    }


    //triggers while an object is inside the "cylinder"
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isInCylinder = true;
        }

        // 1. Get the multiplier directly from the resistance component
        AOE_Resistance resistance = collision.GetComponent<AOE_Resistance>();
        
        // If no component found, multiplier is 1.0 (100% force)
        float forceMultiplier = (resistance != null) ? resistance.GetForceMultiplier() : 1f;

        // 2. Optimization: If the multiplier is 0, don't bother with the rest of the logic
        if (forceMultiplier <= 0)
        {
            if(_doDebugLog) Debug.Log($"{collision.name} is immune (Multiplier is 0).");
            return;
        }

        GrabbableObject grabbedObj;
        bool isGrabbed = false; //if the object doesn't have GrabbableObject then we still interact with it. Change to true to not interact with them
        if(collision.TryGetComponent<GrabbableObject>(out grabbedObj)){
            isGrabbed = grabbedObj.IsTargeted();
        }

        if(!isGrabbed)
        {
            // Pass 'forceMultiplier' into the effects
            switch (effectNumber)
            {
                case effectsEnum.effect0:
                    EffectList.effect0(collision.gameObject, thrust, forceMultiplier);
                    break;
                case effectsEnum.effect1:
                    EffectList.effect1(collision.gameObject, thrust, forceMultiplier);
                    break;
                case effectsEnum.effect2:
                    EffectList.effect2(collision.gameObject, thrust, forceMultiplier);
                    break;
                case effectsEnum.effect3:
                    EffectList.effect3(collision.gameObject, thrust, forceMultiplier);
                    break;
                case effectsEnum.GravityWell:
                    EffectList.GravityWell(collision.gameObject, this.GetComponent<Collider2D>(), thrust, forceMultiplier);
                    break;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isInCylinder = false;
        }
    }

    //a simple setter for the effect enum if you want to change the effect on the fly
    public void setEffect(effectsEnum newEffect){
        effectNumber = newEffect;
        //if someone swaps an effect mid gameplay it will delete the old particle making the new one based on logic below
        if (currentParticle != null) 
        {
            Destroy(currentParticle);
            currentParticle = null;
        }
        //if effectParticles isnt empty get particle prefab based on the effectnumber and then spawn the particle on the AOE cylinder
        int idx = (int)effectNumber;
        if (effectParticles != null && idx >= 0 && idx < effectParticles.Length) 
        {
            GameObject chosenParticle = effectParticles[idx];
            currentParticle = Instantiate(chosenParticle, transform.position, chosenParticle.transform.rotation);
            currentParticle.transform.SetParent(transform);

        }
    }
}
