using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

[RequireComponent(typeof(Rigidbody2D))]
//using static UnityEditor.Searcher.SearcherWindow.Alignment; //what is this


/* -----------------------------------------------------------
 * Author:
 * Alfaroqomar Alaa
 * 
 * 
 * Breakable script that breaks objects into "chunks" when health reaches 0
 * 
 * TODO: Create algo for making different chunk shapes, currently only squares..
 * Apply force based on where last instance of damage came from
 * Gets laggy at higher chunk values, could move to start and only release chunks when destroyed
 * 
 * Decide whether or not chunks can interact with environment??
 * Should chunks be grabbable?
 * 
 * 
 * Bugs:
 * If box is being grabbed and gets destroyed, bugs out, minor
 */// --------------------------------------------------------

public class Breakable : MonoBehaviour, IDamageable
{

    private GameObject breakableObj;
    public int maxHealth = 3;
    private int currentHealth;
    private bool destroyed = false;
    public float chunks = 3f; //^2
    private List<GameObject> myChunks = new();
    private Vector2 collisionPoint;
    private Vector2 collisionVelocity;
    [HideInInspector]
    public float objMass;
    public float fadeTime = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        currentHealth = maxHealth;
        objMass = GetComponent<Rigidbody2D>().mass;
        print("Object mass: " + objMass);
        //print("Breakable box ready with health: " + currentHealth);
    }

    // Update is called once per frame

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) //testing 
        {
            boxDied();
        }

        //print("im prnting");
    }

    private void throwEffects() // need 2 change function name
    {
        //release particles maybe play coroutine
        //play sound
        SpriteRenderer SR = gameObject.GetComponent<SpriteRenderer>();
        Sprite newSprite = Sprite.Create(SR.sprite.texture, SR.sprite.textureRect, new Vector2(0.5f, 0.5f), pixelsPerUnit: SR.sprite.pixelsPerUnit); //gets the box from the tileset
        GameObject Clone = Instantiate(gameObject, gameObject.transform.parent);
        Physics2D.IgnoreCollision(Clone.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
        Clone.GetComponent<SpriteRenderer>().sprite = newSprite;

        Breakable cloneBreakable = Clone.GetComponent<Breakable>();

        cloneBreakable.objMass = objMass;
        cloneBreakable.collisionPoint = collisionPoint;
        cloneBreakable.collisionVelocity = collisionVelocity;


        Clone.GetComponent<Rigidbody2D>().linearVelocity = GetComponent<Rigidbody2D>().linearVelocity;
        Clone.GetComponent<Breakable>().breakObj(Clone);
        //SetVerticesDirty();


    }

    //protected override void OnPopulateMesh(VertexHelper vh)
    //{
    //    if (!destroyed)
    //        return;

    //    List<UIVertex> vertices = new List<UIVertex>();
    //    Transform[] me = new Transform[10 * 10 * 10];
    //    var vertex = UIVertex.simpleVert;
    //    vertex.color = new Color(255, 255, 255, 255);

    //    //initial vertex
    //    vertex.position = new Vector2(0, 0);
    //    vertex.uv0 = new Vector2(vertex.position.x, vertex.position.y);
    //    vertices.Add(vertex);


    //    //ignore this function
    //    //vh.AddUIVertexStream(vertices, indices);


    //}



    private void boxDied()
    {
        destroyed = true;
        throwEffects();
        Destroy(gameObject);

        // play sound
        // play animation
        // spawn loot
        //Destroy(box);
    }

    public void TakeDamage(int damage, Rigidbody2D source)
    {
        if (source != null)
        {
            collisionPoint = source.position;
            collisionVelocity = source.linearVelocity;
            //objMass = rb.mass;
            print("collision point: " + collisionPoint.ToString());
        }
            

        currentHealth -= damage;

        if (currentHealth <= 0 && !destroyed)
        {
            boxDied();
        }
    }

    public void breakObj(GameObject objToBreak)
    {
        print(objToBreak.name);

        for (float i = 0; i < chunks; i++)
        {
            for (float j = 0; j < chunks; j++)
            {
                GameObject chunkObj = Instantiate(objToBreak);
                chunkObj.layer = LayerMask.NameToLayer("Chunk");
                chunkObj.GetComponent<Rigidbody2D>().includeLayers = LayerMask.GetMask("Terrain"); //make sure chunks can interact with environment and grabbable
                ////ignore all layers except terrain
                chunkObj.GetComponent<Rigidbody2D>().excludeLayers = (LayerMask.GetMask("Physics Objects"));
                Destroy(chunkObj.GetComponent<GrabbableObject>());
                myChunks.Add(chunkObj);
                //remove this script from the new chunk
                Destroy(chunkObj.GetComponent<Breakable>());
                chunkObj.name = "Chunk " + i.ToString() + j.ToString();

                #region dividing
                Sprite cloneSprite = chunkObj.GetComponent<SpriteRenderer>().sprite;
                print("Original sprite rect: " + cloneSprite.rect.ToString()); //because we're getting sprite from tilemap, need to store the ogrect then modify it to get full obj?

                // rect x is position of box, 0 is leftmost point
                // rect y is bottommost point, also 0
                //width starts from 0, so does y
                // need to define width from chunks, then position 



                float sprHeight = cloneSprite.rect.height;
                float sprWidth = cloneSprite.rect.width;

                float chunkWidthBegin = sprWidth / chunks * i;
                float chunkWidthEnd = sprWidth / chunks * (i + 1);

                float chunkHeightBegin = sprHeight / chunks * j;
                float chunkHeightEnd = sprHeight / chunks * (j + 1);


                float uMin = chunkWidthBegin / sprWidth;
                float uMax = chunkWidthEnd / sprWidth;
                float vMin = chunkHeightBegin / sprHeight;
                float vMax = chunkHeightEnd / sprHeight;

                Vector2[] myUv = new Vector2[] {
                    new Vector2(uMin, vMin),
                    new Vector2(uMin, vMax),
                    new Vector2(uMax, vMax),
                    new Vector2(uMax, vMin)
                };

                Rect chunkRect = new Rect(
                    cloneSprite.rect.x + i * (cloneSprite.rect.width / chunks),
                    cloneSprite.rect.y + j * (cloneSprite.rect.height / chunks),
                    cloneSprite.rect.width / chunks,
                    cloneSprite.rect.height / chunks
                );



                Sprite tempSprite = Sprite.Create(
                    cloneSprite.texture,
                    chunkRect,
                    new Vector2(0.5f, 0.5f),
                    cloneSprite.pixelsPerUnit
                );

                Vector2[] verts = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, chunkRect.height),
                    new Vector2(chunkRect.width, chunkRect.height),
                    new Vector2(chunkRect.width, 0)
                };

                ushort[] triangles = new ushort[] { 0, 1, 2, 0, 2, 3 };

                tempSprite.OverrideGeometry(verts, triangles);
                Vector2[] colliderPoints = tempSprite.vertices;
                #endregion
                PolygonCollider2D poly;

                if (objToBreak.TryGetComponent<Collider2D>(out _)) // might be a waste of memory?, maybe need to change from boxcollider2d to collider2d
                {
                    Destroy(chunkObj.GetComponent<Collider2D>());
                    poly = chunkObj.AddComponent<PolygonCollider2D>();
                }
                else
                {
                    poly = chunkObj.GetComponent<PolygonCollider2D>();
                }
                
                poly.SetPath(0, colliderPoints);
                chunkObj.GetComponent<SpriteRenderer>().sprite = tempSprite;


                //if chunk doesnt have rb2d add one
                Rigidbody2D chunkRB;
                if (!objToBreak.TryGetComponent<Rigidbody2D>(out _))
                {
                    chunkRB = chunkObj.AddComponent<Rigidbody2D>();
                }
                else
                {
                    chunkRB = chunkObj.GetComponent<Rigidbody2D>();
                }
                chunkRB.useAutoMass = false;
                chunkRB.mass = objMass / (chunks * chunks);
                print("Obj mass " + objMass.ToString() + " chunks " + chunks.ToString());
                print("chunk mass: " + chunkRB.mass);

                if (objToBreak.TryGetComponent<Rigidbody2D>(out Rigidbody2D originalRB))
                {
                    //print("og has rb2d");
                    Physics2D.IgnoreCollision(objToBreak.GetComponent<Collider2D>(), poly, true);
                    chunkRB.linearVelocity = objToBreak.GetComponent<Rigidbody2D>().linearVelocity 
                                           + Vector2.right * UnityEngine.Random.Range(-3, 3)
                                           + Vector2.up * UnityEngine.Random.Range(-1, 1);


                }



                //set chunk pos relative to original obj
                Vector3 ogPos = objToBreak.transform.position;
                float chunkWidth = cloneSprite.bounds.size.x / chunks;
                float chunkHeight = cloneSprite.bounds.size.y / chunks;

                chunkObj.transform.position = new Vector3(
                    ogPos.x - cloneSprite.bounds.size.x / 2f + (i * chunkWidth) + chunkWidth / 2f,
                    ogPos.y - cloneSprite.bounds.size.y / 2f + (j * chunkHeight) + chunkHeight / 2f,
                    ogPos.z
                );

                //chunkRB.constraints = RigidbodyConstraints2D.FreezeAll;

            }
        }
        //Destroy(objToBreak);
        StartCoroutine(DestroyChunk(objToBreak));
        //WaitForSeconds wait = new WaitForSeconds(fadeTime);
        //Destroy(objToBreak);


        if (collisionPoint != Vector2.zero)
        {
            ApplyImpact();
        }
        else
        {
            print("no collision point");
        }

    }

    //destroy chunk coroutine for fadeout
    private IEnumerator DestroyChunk(GameObject cloneObj)
    {
        print("starting destroy coroutine");
        cloneObj.GetComponent<Collider2D>().enabled = false;
        cloneObj.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        cloneObj.GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(fadeTime);
        print("fading chunks");
        for (int i = 0; i < myChunks.Count; i++)
        {
            print("destroying chunk " + i.ToString());
            Destroy(myChunks[i]);
        }

    }

    private void ApplyImpact()
    {
        //get the closest chunk to the collision point, apply force to that chunk from collision obj direction



        GameObject closestChunk = null;
        for (int i = 0; i < myChunks.Count; i++)
        {
            print(i);
            if (closestChunk == null)
            {
                closestChunk = myChunks[i];
            } else if (Vector2.Distance(collisionPoint, (Vector2)myChunks[i].transform.position) < Vector2.Distance(collisionPoint, (Vector2)closestChunk.transform.position))
            {
                print(myChunks[i].transform.position);

                closestChunk = myChunks[i];
            }
        }

        float calculatedForce = 7f;

        //bug, closest chunk is always 0,0 this is cuz rect is still the size of original obj

        closestChunk.GetComponent<Rigidbody2D>().AddForce(((Vector2)closestChunk.transform.position - collisionPoint).normalized * calculatedForce, ForceMode2D.Impulse);
        print("applied " + calculatedForce.ToString() + " force to " + closestChunk.name);


    }

    private void ApplyRandomImpact()
    {
        for (int i = 0; i < myChunks.Count; i++)
        {
            float calculatedForce = 7f;

            myChunks[i].GetComponent<Rigidbody2D>().AddForce(UnityEngine.Random.insideUnitCircle.normalized * calculatedForce, ForceMode2D.Impulse);
        }

        
    }

}