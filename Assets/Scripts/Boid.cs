using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
	// NOTE: This is -somewhat- stolen. Credit: Sebastian Lague
	
	[SerializeField] private BoidManager manager;
	[SerializeField] private BoidSettings settings;

	[SerializeField] private float sqrViewDistance = 4f;
	[SerializeField] private float viewAngle = 135f;
	
	[HideInInspector] public Vector3 position;
	public Vector3 forward;
    public Vector3 velocity;


    private List<Boid> currentFlock = new List<Boid>();
    private int numOfFlock;
    public Vector3 centreOfFlock;
    private Vector3 directionOfFlock;
    private Vector3 acceleration;

    private Material material;
    private Transform cashedTransform;

    private void Awake()
    {
	    manager = FindObjectOfType<BoidManager>();
	    material = transform.GetComponentInChildren<MeshRenderer>().material;
	    position = transform.position;
    }

    public void Initialize(BoidSettings settings, Transform target)
    {
	    this.settings = settings;

	    float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
	    velocity = transform.forward * startSpeed;
    }
    
    public void UpdateBoid()
    {
	    acceleration = Vector3.zero;
	    SearchForFlock();
	    CalculateFlockData();

	    print(numOfFlock);
	    
	    if (numOfFlock != 0)
	    {
		    Vector3 offsetToFlockCentre = centreOfFlock - position;
		    
		    Vector3 alignmentForce = SteerTowards(directionOfFlock) * settings.alignWeight;
		    Vector3 cohesionForce = SteerTowards(offsetToFlockCentre) * settings.cohesionWeight;
		    Vector3 seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;
		    
		    // NOTE: Acceleration and force are two different physical quantities and can't be added together.
		    acceleration += alignmentForce;
		    acceleration += cohesionForce;
		    //acceleration += seperationForce;

		    if (IsHeadingForCollision())
		    {
			    Vector3 collisionAvoidDir = ObstacleRays();
			    print(Vector3.Angle(forward, collisionAvoidDir));
			    Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
			    acceleration += collisionAvoidForce;
		    }
	    }
	    
	    // This is to clamp the speed
	    velocity += acceleration * Time.fixedDeltaTime;
	    float speed = velocity.magnitude;
	    Vector3 dir = velocity / speed;
	    speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
	    velocity = dir * speed;

	    position += velocity * Time.fixedDeltaTime;
	    forward = dir;

	    this.transform.position += velocity * Time.fixedDeltaTime;
	    transform.forward = forward;
    }

    
    void SearchForFlock()
    {
	    currentFlock.Clear();
	    
	    foreach (Boid b in manager.boids)
	    {
		    
		    Vector3 boidDirection = position - b.position;
		    if (boidDirection.sqrMagnitude < sqrViewDistance)
		    {
			    if (Vector3.Angle(forward, boidDirection) < viewAngle)
			    {
				    currentFlock.Add(b);
			    }
		    }
	    } 
    }

    void CalculateFlockData()
    {
	    // Might be better to do it in SearchForFlock() right away
	    numOfFlock = 0;
	    centreOfFlock = Vector3.zero;
	    directionOfFlock = Vector3.zero;

	    foreach (Boid b in currentFlock)
	    {
		    numOfFlock++;
		    centreOfFlock += b.position;
		    directionOfFlock += b.forward;
	    }

	    centreOfFlock /= numOfFlock;
	    directionOfFlock /= numOfFlock;
    }

    bool IsHeadingForCollision()
    {
	    RaycastHit hit;
	    if (Physics.SphereCast(position, settings.boundsRadius, forward, out hit, settings.collisionAvoidDst))
	    {
		    print("Heading for collision");
		    return true;
	    }
	    else
	    {
		    print("Clear path");
		    return false;
	    }
    }
    
    Vector3 ObstacleRays()
    {
	    Vector3[] rayDirections = BoidHelper.directions;

	    for (int i = 0; i < rayDirections.Length; i++)
	    {
		    Vector3 dir = transform.TransformDirection((rayDirections[i]));
		    
		    Ray ray = new Ray(position, dir);
		    if (!Physics.SphereCast(ray, settings.boundsRadius, settings.collisionAvoidDst))
		    {
			    return dir;
		    }
	    }

	    return forward;
    }

    Vector3 SteerTowards(Vector3 vector)
    {
	    Vector3 v = vector.normalized * settings.maxSpeed - velocity;
	    return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }
    
    public void SetColor(Color color)
    {
	    material.color = color;
    }

}
