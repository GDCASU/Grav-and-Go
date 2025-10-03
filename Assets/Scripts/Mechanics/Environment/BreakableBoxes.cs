using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

public class Breakable : Graphic, IDamageable
{

    private GameObject breakableObj;
    private int maxHealth = 3;
    private int currentHealth;
    private bool destroyed = false;
    public float chunks = 3f; //range 2-10, exponential performance cost

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    new void Start()
    {
        currentHealth = maxHealth;
        //print("Breakable box ready with health: " + currentHealth);
    }

    // Update is called once per frame

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) //testing 
        {
            boxDied();
        }
    }

    private void throwEffects() // need 2 change function name
    {
        //release particles maybe play coroutine
        //play sound
        SpriteRenderer SR = gameObject.GetComponent<SpriteRenderer>();
        Sprite newSprite = Sprite.Create(SR.sprite.texture, SR.sprite.textureRect, new Vector2(0.5f, 0.5f), pixelsPerUnit:SR.sprite.pixelsPerUnit); //gets the box from the tileset
        GameObject Clone = Instantiate(gameObject, gameObject.transform.parent);
        Clone.GetComponent<SpriteRenderer>().sprite = newSprite;
        Clone.GetComponent<Breakable>().breakObj(Clone);
        SetVerticesDirty();


    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (!destroyed)
            return;

        List<UIVertex> vertices = new List<UIVertex>();
        Transform[] me = new Transform[10 * 10 * 10];
        var vertex = UIVertex.simpleVert;
        vertex.color = new Color(255, 255, 255, 255);

        //initial vertex
        vertex.position = new Vector2(0, 0);
        vertex.uv0 = new Vector2(vertex.position.x, vertex.position.y);
        vertices.Add(vertex);
        

        //ignore this function
        //vh.AddUIVertexStream(vertices, indices);


    }



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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0 && !destroyed)
        {
            boxDied();
        }
    }

    public void breakObj(GameObject objToBreak)
    {
        for (float i = 0; i < chunks; i++)
        {
            for (float j = 0; j < chunks; j++)
            {
                GameObject chunkObj = Instantiate(objToBreak);
                //remove this script from the new chunk
                Destroy(chunkObj.GetComponent<Breakable>());
                chunkObj.name = "Chunk " + i;

                Sprite cloneSprite = chunkObj.GetComponent<SpriteRenderer>().sprite;
                Sprite sprite = Sprite.Create(
                    cloneSprite.texture,
                    cloneSprite.rect,
                    cloneSprite.pivot / cloneSprite.rect.size,
                    cloneSprite.pixelsPerUnit
                    );


                float chunkWidth = sprite.rect.width / chunks;
                float chunkHeight = sprite.rect.height / chunks;

                float sprHeight = sprite.rect.height;
                float sprWidth = sprite.rect.width;

                float chunkWidthBegin = sprWidth / chunks * i;
                float chunkWidthEnd = sprWidth / chunks * (i + 1);

                float chunkHeightBegin = sprHeight / chunks * j;
                float chunkHeightEnd = sprHeight / chunks * (j + 1);


                Vector2[] vertices = new Vector2[4];
                Vector2[] uv = new Vector2[4]; //uvs are the same number as the vertices..? at least what video said
                ushort[] triangles = new ushort[6];

                vertices[0] = new Vector3(chunkWidthBegin, chunkHeightBegin);
                vertices[1] = new Vector3(chunkWidthBegin, chunkHeightEnd);
                vertices[2] = new Vector3(chunkWidthEnd, chunkHeightEnd);
                vertices[3] = new Vector3(chunkWidthEnd, chunkHeightBegin);

                uv[0] = new Vector2(0, 0);
                uv[1] = new Vector2(0, 1);
                uv[2] = new Vector2(1, 1);
                uv[3] = new Vector2(1, 0);

                triangles[0] = 0;
                triangles[1] = 1;
                triangles[2] = 2;

                triangles[3] = 0;
                triangles[4] = 2;
                triangles[5] = 3;

                sprite.OverrideGeometry(vertices, triangles);
                Vector2[] colliderPoints = sprite.vertices;
                PolygonCollider2D poly;

                if (objToBreak.TryGetComponent<BoxCollider2D>(out _)) // might be a waste of memory?
                {
                    Destroy(chunkObj.GetComponent<BoxCollider2D>());
                    poly = chunkObj.AddComponent<PolygonCollider2D>();
                } else
                {
                    poly = chunkObj.GetComponent<PolygonCollider2D>();
                }

                poly.SetPath(0, colliderPoints);


                chunkObj.GetComponent<SpriteRenderer>().sprite = sprite;
                if (!objToBreak.TryGetComponent<Rigidbody2D>(out _))
                    chunkObj.AddComponent<Rigidbody2D>();
            }
        }
        Destroy(objToBreak);
    }

}
