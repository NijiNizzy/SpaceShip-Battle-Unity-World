using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Niko Bazos (ndb9897@rit.edu)
 * The purpose of this class is to enacpsulate the objects in spheres to simulate collision.
 */

public class BoundingSphere : MonoBehaviour
{
	// Attributes
	private Vector3 position;
	public float radius = 1.0f;
	public bool colliding = false;

	// Use this for initialization
	void Start ()
	{
		if (radius <= 0.0f)
		{
			radius = 1.0f;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		position = gameObject.transform.position;
	}

	/*
	void OnDrawGizmos()
	{
		if (colliding)
		{
			Gizmos.color = new Color (1.0f, 0.0f, 0.0f, 0.33f);
		}
		else
		{
			Gizmos.color = new Color (1.0f, 1.0f, 0.0f, 0.50f);
		}
		Gizmos.DrawSphere(position, radius);
	}
	*/

	// Checks for collision using the radii of the two specified objects
	public bool IsColliding(BoundingSphere other)
	{
		bool output = false;
		if(radius + other.radius > Vector3.Distance(position, other.position))
		{
			output = true;
		}
		return output;
	}
}