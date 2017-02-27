using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Niko Bazos (ndb9897@rit.edu)
 * The purpose of this class is to manage the behaviors of all in-game objects.
 */

public class BehaviourManager : MonoBehaviour
{
	// Attributes 
	public GameObject followerPrototype; //vehicle to instantiate
	public GameObject leaderPrototype; //target to instantiate
	public GameObject alienShipPrototype;
	public GameObject alienLeaderShipPrototype;
	public GameObject motherShipLeaderPrototype;
	public GameObject motherShipFollowerProtoype;

	// Flocker lists
	public List<GameObject> followers;
	public float followerCount;

	public List<GameObject> alienShips;
	public float alienShipCount;

	public List<GameObject> motherShips;
	public float motherShipCount;

	// Target lists
	public List<GameObject> targets;
	public List<GameObject> alienShipTargets;
	public List<GameObject> motherShipTargets;

	// Leader GameObjects
	private GameObject leader;
	private GameObject alienShipLeader;
	private GameObject motherShip;

	// Other Classes
	LeaderMovementForces leaderMF;
	ShipLeaderMovementForces alienShipLeaderMF;
	ShipLeaderMovementForces motherShipMF;
	Path point;
	GameObject gmngr;

	// indeces
	private int robotTargetIndex;
	private int alienShipTargetIndex;
	private int motherShipTargetIndex;

	// Terrain
	public Terrain terrain; //terrain we are walking in
	public TerrainGenerator terrainGenerator; //terrain information
	public Vector3 worldSize; //world size

	// SmoothFollow objects
    public GameObject cameraObj; //Representation of centroid that the camera will be attached to
	public GameObject cameraObj2;
	public GameObject cameraObj3;
	public GameObject cameraObj4;
	public GameObject cameraObj5;
	public GameObject cameraObj6;

	// Obstacles (REMEMBER YOU CAN USE ALL MODELS BY PLACING THEM IN THE SCENE)
	public int obstCount;
	public List<GameObject> obstList;

	// Average position and direction of flock
	private Vector3 robCentroid;
	private Vector3 robDirection;
	private Vector3 alienCentroid;
	private Vector3 alienDirection;
	private Vector3 motherShipCentroid;
	private Vector3 motherShipDirection;


	public Material matRed;


	// Use this for initialization
	void Start ()
	{
		//is vehicle prototype assigned in editor?
		if(null == followerPrototype)
		{
			Debug.Log("Error in " + gameObject.name + 
			          ": VehiclePrototype is not assigned.");
			Debug.Break();
		}
		//is target prototype assigned in editor?
		if(null == leaderPrototype)
		{
			Debug.Log("Error in " + gameObject.name + 
			          ": VehiclePrototype is not assigned.");
			Debug.Break();
		}
		//is terrain assigned in editor?
		if(null == terrain)
		{
			Debug.Log("Error in " + gameObject.name + 
			          ": Terrain is not assigned.");
			Debug.Break();
		}
		//is the terrain assigned a terraingenerator component?
		terrainGenerator = terrain.GetComponent<TerrainGenerator>();
		if(null == terrainGenerator)
		{
			Debug.Log("Error in " + gameObject.name + 
			          ": Terrain is required to have a TerrainGenerator script");
			Debug.Break();
		}

		// Initialize this world size with the terrain generator world size
		worldSize = terrainGenerator.worldSize;

		gmngr = GameObject.Find("GameManager");

		point = gmngr.GetComponent<Path> ();

		// Initialize indeces
		alienShipTargetIndex = 0;
		motherShipTargetIndex = 0;
		robotTargetIndex = 0;

		// Instantiate and randomly place robot leader
		leader = Instantiate (leaderPrototype);
		RandomizePosition (leader);

		leaderMF = leader.GetComponent<LeaderMovementForces> ();
		leaderMF.SetTarget (targets [0].transform.position);
		leaderMF.SetObstList (obstList);
		leaderMF.SetTargetList (followers);

		// Instantiate and randomly place alien ship leader
		alienShipLeader = Instantiate(alienLeaderShipPrototype);
		RandomizeShipPosition (alienShipLeader);

		alienShipLeaderMF = alienShipLeader.GetComponent<ShipLeaderMovementForces> ();
		alienShipLeaderMF.SetTarget (alienShipTargets[alienShipTargetIndex].transform.position);
		alienShipLeaderMF.SetObstList (obstList);
		alienShipLeaderMF.SetTargetList (alienShips);

		// Instantiate and place mothership
		motherShip = (GameObject)Instantiate(motherShipLeaderPrototype, new Vector3(100,350,100),Quaternion.identity);

		motherShipMF = motherShip.GetComponent<ShipLeaderMovementForces> ();
		motherShipMF.SetTarget (motherShipTargets[motherShipTargetIndex].transform.position);
		motherShipMF.SetObstList (obstList);
		motherShipMF.SetTargetList (motherShips);


		// Instantiate and randomly place robot followers and set their target
		// along with the list of obstacles and the own list.
		for (int i = 0; i < followerCount; i++) 
		{
			followers.Add(Instantiate (followerPrototype));
			RandomizePosition (followers[i]);

			FlockerMovementForces followerMF = followers[i].GetComponent<FlockerMovementForces> ();
			followerMF.SetTarget (leader.transform.position);
			followerMF.SetObstList (obstList);
			followerMF.SetTargetList (followers);
		}
			
		// Instantiate and randomly place alien ship followers and set their target
		// along with the list of obstacles and the own list.
		for (int i = 0; i < alienShipCount; i++) 
		{
			alienShips.Add(Instantiate (alienShipPrototype));
			RandomizeShipPosition (alienShips[i]);

			ShipFlockerMovementForces alienMF = alienShips[i].GetComponent<ShipFlockerMovementForces> ();
			alienMF.SetTarget (alienShipLeader.transform.position);
			alienMF.SetObstList (obstList);
			alienMF.SetTargetList (alienShips);
		}

		// Instantiate and randomly place mothership followers and set their target
		// along with the list of obstacles and the own list.
		for (int i = 0; i < motherShipCount; i++) 
		{
			motherShips.Add((GameObject)Instantiate (motherShipFollowerProtoype, new Vector3(100,350,100), Quaternion.identity));

			ShipFlockerMovementForces motherShipsMF = motherShips[i].GetComponent<ShipFlockerMovementForces> ();
			motherShipsMF.SetTarget (motherShip.transform.position);
			motherShipsMF.SetObstList (obstList);
			motherShipsMF.SetTargetList (motherShips);
		}
	}
		
	void RandomizePosition(GameObject theObject)
	{
		//Set position of target based on the size of the world
		Vector3 position = new Vector3 (Random.Range(0.0f,100.0f), 0.0f, Random.Range(0.0f,100.0f));
		//set the height of the object based on the position of the terrain
		position.y = terrainGenerator.GetHeight(position) + 2.0f;
		//set the position of target back
		theObject.transform.position = position;
	}

	void RandomizeShipPosition(GameObject theObject)
	{
		//Set position of target based on the size of the world
		Vector3 position = new Vector3 (Random.Range(300.0f,400.0f), 50.0f, Random.Range(200.0f,300.0f));
		//set the position of target back
		theObject.transform.position = position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Initialize the average position and direction
		Vector3 robSumPos = Vector3.zero;
		Vector3 robSumVel = Vector3.zero;
		Vector3 shipSumPos = Vector3.zero;
		Vector3 shipSumVel = Vector3.zero;
		Vector3 motherShipSumPos = Vector3.zero;
		Vector3 motherShipSumVel = Vector3.zero;

		// Loop through the list of robot followers
		for (int i = 0; i < followers.Count; i++) 
		{
			FlockerMovementForces followerMF = followers[i].GetComponent<FlockerMovementForces> ();
			followerMF.SetTarget (leader.transform.position);
			// Add the sum of the positions and velocities
			robSumPos += followers [i].transform.position;
			robSumVel += followers [i].GetComponent<FlockerMovementForces> ().velocity;
		}

		// Loop through the list of alien ship followers
		for (int i = 0; i < alienShips.Count; i++) 
		{
			ShipFlockerMovementForces alienMF = alienShips[i].GetComponent<ShipFlockerMovementForces> ();
			alienMF.SetTarget (alienShipLeader.transform.position);
			// Add the sum of the positions and velocities
			shipSumPos += alienShips [i].transform.position;
			shipSumVel += alienShips [i].GetComponent<ShipFlockerMovementForces> ().velocity;
		}

		// Loop through the list of alien ship followers
		for (int i = 0; i < motherShips.Count; i++) 
		{
			ShipFlockerMovementForces motherShipsMF = motherShips[i].GetComponent<ShipFlockerMovementForces> ();
			motherShipsMF.SetTarget (motherShip.transform.position);
			// Add the sum of the positions and velocities
			motherShipSumPos += motherShips [i].transform.position;
			motherShipSumVel += motherShips [i].GetComponent<ShipFlockerMovementForces> ().velocity;
		}

		// Simple path following for the robot leader
		if (leader.GetComponent<BoundingSphere> ().IsColliding (targets[robotTargetIndex].GetComponent<BoundingSphere> ())) 
		{
			if (robotTargetIndex < targets.Count - 1) 
			{
				robotTargetIndex++;
				leaderMF.SetTarget (targets [robotTargetIndex].transform.position);
			} 
			else 
			{
				robotTargetIndex = 0;
				leaderMF.SetTarget (targets [robotTargetIndex].transform.position);
			}
		}

		//Complex path following call
		//leaderMF.SetTarget (point.ClosestPoint (leader));

		// Simple path following for the alien ship leader
		if (alienShipLeader.GetComponent<BoundingSphere> ().IsColliding (alienShipTargets [alienShipTargetIndex].GetComponent<BoundingSphere> ())) 
		{
			if (alienShipTargetIndex < alienShipTargets.Count - 1) 
			{
				alienShipTargetIndex++;
				alienShipLeaderMF.SetTarget (alienShipTargets[alienShipTargetIndex].transform.position);
			} 
			else 
			{
				alienShipTargetIndex = 0;
				alienShipLeaderMF.SetTarget (alienShipTargets[alienShipTargetIndex].transform.position);
			}			
		}

		// Simple path following for the mothership leader
		if (motherShip.GetComponent<BoundingSphere> ().IsColliding (motherShipTargets [motherShipTargetIndex].GetComponent<BoundingSphere> ())) 
		{
			if (motherShipTargetIndex < motherShipTargets.Count - 1) 
			{
				motherShipTargetIndex++;
				motherShipMF.SetTarget (motherShipTargets[motherShipTargetIndex].transform.position);
			} 
			else 
			{
				motherShipTargetIndex = 0;
				motherShipMF.SetTarget (motherShipTargets[motherShipTargetIndex].transform.position);
			}			
		}




		// Get the averages
		robCentroid = robSumPos / followers.Count;
		robDirection = robSumVel / followers.Count;

		// Get the camera position behind the leader
		Vector3 behindLeader = leader.transform.position - leaderMF.velocity/10.0f;

		// Set the y of the centroid position to the height of the terrain
		robCentroid.y = terrainGenerator.GetHeight(robCentroid) + 2.0f;
		cameraObj.transform.position = robCentroid;
		cameraObj2.transform.position = behindLeader;

		// Get the averages
		alienCentroid = shipSumPos / alienShips.Count;
		alienDirection = shipSumVel / alienShips.Count;

		// Get the camera position behind the leader
		Vector3 behindShipLeader = alienShipLeader.transform.position - alienShipLeaderMF.velocity/10.0f;

		// Set the y of the centroid position 
		cameraObj3.transform.position = alienCentroid;
		cameraObj4.transform.position = behindShipLeader;

		// Get the averages
		motherShipCentroid = motherShipSumPos / motherShips.Count;
		motherShipDirection = motherShipSumVel / motherShips.Count;

		// Get the camera position behind the leader
		Vector3 behindMotherShipLeader = motherShip.transform.position - motherShipMF.velocity/10.0f;

		// Set the y of the centroid position 
		cameraObj5.transform.position = motherShipCentroid;
		cameraObj6.transform.position = behindMotherShipLeader;


	}

	/*
	void OnRenderObject()
	{
		GL.PushMatrix ();

		// Direction line
		this.matRed.SetPass (0);
		GL.Begin (GL.LINES);
		GL.Vertex (robCentroid);
		GL.Vertex (robCentroid + robDirection.normalized * 3.0f);
		GL.End ();

		GL.PopMatrix ();

	}
	*/
}
