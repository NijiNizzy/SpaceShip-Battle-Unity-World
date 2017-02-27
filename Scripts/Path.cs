using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Niko Bazos (ndb9897@rit.edu)
 * This class manages complex path following. 
 */

public class Path : MonoBehaviour 
{
	//Attributes
	public List <GameObject> waypoints;
	public float radius;

	// Use this for initialization
	void Start () 
	{
		radius = 1.0f;
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Debug lines
		for (int i = 0; i < waypoints.Count - 1; i++) 
		{
			Debug.DrawLine (waypoints [i].transform.position, waypoints [i + 1].transform.position,Color.green,2.0f,false);
		}

		Debug.DrawLine (waypoints [0].transform.position, waypoints [waypoints.Count-1].transform.position,Color.green,2.0f,false);
	
	}

	// Complex path following algorithim
	public Vector3 ClosestPoint(GameObject leader)
	{
		// Needed values
		Vector3 target = Vector3.zero;
		Vector3 seg = Vector3.zero;
		Vector3 BFP = Vector3.zero;
		float leastDistance = 0.0f;
		int index = 0;
		bool firstWaypoint = true;

		// Loops through the waypoints and finds the closest waypoint to the GameObject
		for (int i = 0; i < waypoints.Count - 1; i++) 
		{
			float distance = Vector3.Distance (leader.transform.position, waypoints [i].transform.position);

			if (firstWaypoint) 
			{
				leastDistance = distance;
				index = i;
				firstWaypoint = false;
			} 
			else 
			{
				if (distance < leastDistance) 
				{
					leastDistance = distance;
					index = i;
				} 
			}
		}


		// Finds the segment based on the closest waypoint
		if (index < waypoints.Count - 1) 
		{
			seg = waypoints [index].transform.position + (waypoints [index + 1].transform.position - waypoints [index].transform.position);
		} 
		else 
		{
			seg = waypoints [5].transform.position + (waypoints [0].transform.position - waypoints [5].transform.position);
		}

		// Beginning to future point vector
		BFP = waypoints [index].transform.position + (leader.GetComponent<LeaderMovementForces> ().velocity - leader.transform.position);
		
		// Scalar Projection
		float scalarProjection = Vector3.Dot (BFP, seg);

		// Closest Point Calculation
		Vector3 closestPoint = waypoints [index].transform.position + seg.normalized * scalarProjection;

		// Distance away from path
		float distanceFromPath = Vector3.Distance ((leader.GetComponent<LeaderMovementForces> ().velocity - leader.transform.position), closestPoint);


		// Determining if the GameObject is in or out of the path and decided what it should seek.
		bool inPath = false;

		if (distanceFromPath > radius) 
		{
			target = closestPoint;
		} 
		else 
		{
			inPath = true;
		}


		if (inPath) 
		{
			if (index < waypoints.Count - 1) 
			{
				target = waypoints [index + 1].transform.position;
			} 
			else 
			{
				target = waypoints [0].transform.position;
			}

			inPath = false;
		}
			
		return target;
	}

}
