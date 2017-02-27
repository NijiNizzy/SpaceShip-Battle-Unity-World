using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Niko Bazos (ndb9897@rit.edu)
 * This class manages the movement forces of ship leaders
 */

public class ShipLeaderMovementForces : MovementForces 
{
	// Attributes
	public float seekWeight;
	public float fleeWeight;
	public float separateWeight;
	public float maxForce;
	public float distance;
	public float safeDistance;
	public float detectDistance;

	// Use this for initialization
	override public void Start () 
	{
		base.Start ();
		maxForce = 100.0f;
		safeDistance = 10.0f;
		detectDistance = 20.0f;
		seekWeight = 50.0f;
		fleeWeight = 30.0f;
		separateWeight = 50.0f;
	}

	// set the closest zombie to flee from
	override public void SetTarget(Vector3 pos)
	{
		targetPosition = pos;
	}

	// sets the list of zombies
	override public void SetTargetList(List<GameObject> targets)
	{
		targetList = targets;
	}

	// sets the bool for turning debug off or on
	override public void SetDebug(bool d)
	{
		debug = d;
	}

	// sets the obstacle list
	override public void SetObstList(List<GameObject> obstacles)
	{
		obstList = obstacles;
	}

	// Update the position based on the velocity and acceleration
	override public void UpdatePosition()
	{
		//update position to current tranform
		position = transform.position;

		// avoids any obstacles
		AvoidObstacle ();

		Vector3 seekingForce = Seek (targetPosition);
		ApplyForce (seekingForce);

		// Calculate the separation force
		Vector3 separationForce = Separate ();
		ApplyForce (separationForce);

		//Step 1: Add Acceleration to Velocity * Time
		velocity += acceleration * Time.deltaTime;
		//Step 2: Add vel to position * Time
		position += velocity * Time.deltaTime;
		//Step 3: Reset Acceleration vector
		acceleration = Vector3.zero;
		//Step 4: Calculate direction (to know where we are facing)
		direction = velocity.normalized;
	}

	// loops through each obstacle and checks the GameObject the script is attached to and checks to avoid
	override public void AvoidObstacle()
	{
		for (int i = 0; i < obstList.Count; i++) 
		{
			// forces to store
			Vector3 steer;
			Vector3 desiredVelocity = Vector3.zero;
			// get radii
			float obstRadius = obstList[i].GetComponent<BoundingSphere> ().radius;
			float agentRadius = gameObject.GetComponentInParent<BoundingSphere> ().radius;

			// Get vector from agent to an obstacle center - vecToCenter
			Vector3 vecToCenter = obstList[i].transform.position - gameObject.transform.position;

			// Remove obstacles too far away from the agent
			if ((vecToCenter.magnitude > detectDistance)) 
			{
				steer = Vector3.zero;
			}
			// Remove obstacles behind the agent
			else if (Vector3.Dot (vecToCenter, gameObject.transform.forward) < 0) 
			{
				steer = Vector3.zero;
			}
			else if ((agentRadius + obstRadius) < Vector3.Dot (vecToCenter, gameObject.transform.right)) 
			{
				steer = Vector3.zero;
			}

			// If obstacles is in the line of sight
			else 
			{
				if (Vector3.Dot (vecToCenter, gameObject.transform.right) > 0) 
				{
					desiredVelocity = -gameObject.transform.right * maxSpeed;
				} 
				else 
				{
					desiredVelocity = gameObject.transform.right * maxSpeed;
				}
			}
			// Determine the steering force
			steer = desiredVelocity - velocity;

			// Increases weight of force
			steer *= detectDistance / vecToCenter.magnitude;

			// Apply the force
			ApplyForce(steer);
		}
	}

	// Separates the objects in the flock
	Vector3 Separate()
	{
		// The amount of separation wanted
		float desiredSeparation = gameObject.GetComponent<BoundingSphere> ().radius * 2.0f;
		Vector3 sum = Vector3.zero;
		Vector3 steer = Vector3.zero;
		int count = 0;

		// Loops through the seeker list and and checks if any other part of the flock is within the range
		for (int i = 0; i < targetList.Count; i++) 
		{
			float d = Vector3.Distance (gameObject.transform.position, targetList [i].transform.position);
			if (d > 0 && d < desiredSeparation) 
			{
				Vector3 difference = gameObject.transform.position - targetList [i].transform.position;
				difference.Normalize ();
				difference = difference / d;
				sum += difference;
				count++;
			}
		}

		// Applies the force if there is a neighbor with the wanted separation
		if (count > 0) 
		{
			sum = sum / count;
			sum.Normalize ();
			sum *= maxSpeed;
			steer = sum - velocity;
		}

		// return the force
		return steer*separateWeight;
	}

	/*
	override public void OnRenderObject()
	{
		if (debug) 
		{
			//velocity
			GL.PushMatrix ();
			behaviourMngr.matGreen.SetPass (0);
			GL.Begin (GL.LINES);
			GL.Vertex (position);
			GL.Vertex (position + direction * 3.0f);			
			GL.End ();

			//To target
			if (null != targetList) 
			{
				foreach (GameObject v in targetList) 
				{
					behaviourMngr.matBlack.SetPass (0);
					GL.Begin (GL.LINES);
					GL.Vertex (position);
					GL.Vertex (v.transform.position);
					GL.End ();
				}
			}

			// right vector
			behaviourMngr.matBlue.SetPass (0);
			GL.Begin (GL.LINES);
			GL.Vertex (position);
			GL.Vertex (position + transform.forward * 3.0f);
			GL.End ();

			GL.PopMatrix ();

		}
	}
	*/

	// for displaying future positions
	public void OnDrawGizmos()
	{
		if (debug) 
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere (transform.position + velocity / 2.0f, 0.25f);
		}
	}

	//Apply a force to the vehicle
	override public void ApplyForce(Vector3 force)
	{
		//F = M * A
		//F / M = M * A / M
		//F / M = A * (M / M)
		//F / M = A * 1
		//A = F / M

		// clamp the force being passed in
		if (force.magnitude > maxForce) 
		{
			force = Vector3.ClampMagnitude(force, maxForce);
		}

		acceleration += force / mass;
	}


	// Pursue
	override public Vector3 Pursuit(Vector3 targetPosition)
	{
		Vector3 distance = targetPosition - position;
		float update = distance.magnitude / maxForce;
		Vector3 futurePosition = targetPosition + target.GetComponent<MovementForces> ().velocity * update;
		return Seek (futurePosition);
	}

	override public Vector3 Seek(Vector3 targetPosition)
	{
		//Step 1: Calculate the desired unclamped velocity
		//which is from this vehicle to target's position
		Vector3 desiredVelocity = targetPosition - position;

		//Step 2: Calculate maximum speed
		//so the vehicle does not move faster than it should
		//desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);

		//Step 2 Alternative:
		desiredVelocity.Normalize ();
		desiredVelocity *= maxSpeed;

		//Step 3: Calculate steering force
		Vector3 steeringForce = desiredVelocity - velocity;

		steeringForce *= seekWeight;

		//Step 4: return the force so it can be applied to this vehicle
		return steeringForce;
	}

	// Evade
	override public Vector3 Evade(Vector3 targetPosition)
	{
		Vector3 distance = targetPosition - position;
		float update = distance.magnitude / maxForce;
		Vector3 futurePosition = targetPosition + target.GetComponent<MovementForces> ().velocity * update;
		return Flee (futurePosition);
	}

	override public Vector3 Flee(Vector3 targetPosition)
	{
		//Step 1: Calculate the desired unclamped velocity
		//which is from this vehicle to target's position
		Vector3 desiredVelocity = position - targetPosition;

		//Step 2: Calculate maximum speed
		//so the vehicle does not move faster than it should
		//desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);

		//Step 2 Alternative:
		desiredVelocity.Normalize ();
		desiredVelocity *= maxSpeed;

		//Step 3: Calculate steering force
		Vector3 steeringForce = desiredVelocity - velocity;

		steeringForce *= fleeWeight;
		//Step 4: return the force so it can be applied to this vehicle
		return steeringForce;
	}

	override public Vector3 Wander()
	{
		// get a random point within the unit sphere
		Vector3 wanderForce = (Random.insideUnitSphere * 50.0f);
		// keep the y to 0
		wanderForce.y = 0.0f;

		// scale the vector
		wanderForce *= seekWeight;

		return wanderForce;
	}

	//Apply friction to the vehicle based on the coefficient
	override public void ApplyFriction(float coeff)
	{
		// Step 1: Oposite velocity
		Vector3 friction = velocity * -1.0f;
		// Step 2: Normalize so is independent of velocity
		friction.Normalize ();
		// Step 3: Multiply by coefficient
		friction = friction * coeff;
		// Step 4: Add friction to acceleration
		acceleration += friction;
	}

	//Apply the trasformation
	override public void SetTransform()
	{
		// keeps the y position constant
		transform.position = position;
		//orient the object
		transform.right = direction;
	}

	// Bounce the object towards the center
	override public void BounceTowardsCenter()
	{	
		// has a buffer zone 5.0f before seeks the center

		//Check within X
		if(position.x > worldSize.x - 5.0f)
		{
			Seek (new Vector3 (worldSize.x / 2.0f, 7.0f, worldSize.z / 2.0f));

			velocity.x *= -1.0f;
		}
		else if(position.x < 5.0f)
		{
			Seek (new Vector3 (worldSize.x / 2.0f, 7.0f, worldSize.z / 2.0f));

			velocity.x *= -1.0f;
		}

		//check within Z
		if(position.z > worldSize.z - 5.0f)
		{
			Seek (new Vector3 (worldSize.x / 2.0f, 7.0f, worldSize.z / 2.0f));

			velocity.z *= -1.0f;
		}
		else if(position.z < 5.0f)
		{
			Seek (new Vector3 (worldSize.x / 2.0f, 7.0f, worldSize.z / 2.0f));

			velocity.z *= -1.0f;
		}
	}


	void Update ()
	{
		UpdatePosition (); //Update the position based on forces
		BounceTowardsCenter(); // keeps forces within park
		SetTransform();//Set the transform before render
	}
}
