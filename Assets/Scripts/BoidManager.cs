using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    // NOTE: This is -somewhat- stolen. Credit: Sebastian Lague

    public BoidSettings settings;
    public Boid[] boids;
    
    void Start()
    {
        boids = FindObjectsOfType<Boid>();
        foreach (Boid b in boids)
        {
            b.Initialize(settings, null);
        }
    }

    void Update()
    {
        if (boids != null)
        {
            //Debug.Log("Test");
            int numBoids = boids.Length;
            
            for(int i = 0; i < numBoids; i++)
            {
                boids[i].UpdateBoid();
            }
        }
    }
}
