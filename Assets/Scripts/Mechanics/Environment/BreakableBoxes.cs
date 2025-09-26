using UnityEngine;

public class BreakableBoxes : MonoBehaviour, IDamageable
{

    private GameObject box;
    private int maxHealth = 3;
    private int currentHealth;
    private bool destroyed = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        


        

    }



    private void throwEffects()
    {
        //release particles maybe play coroutine
        //play sound
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
}
