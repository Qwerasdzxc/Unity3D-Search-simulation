using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeNetworkCreator : MonoBehaviour {

	public int boardWidth = 20;
	public int boardHeight = 15;
	public IDictionary<Vector3, bool> walkablePositions = new Dictionary<Vector3, bool>();
	public IDictionary<Vector3, GameObject> nodeReference = new Dictionary<Vector3, GameObject>();

	[SerializeField]
	public List<Sprite> obstacleSprites;

	// Use this for initialization
	void Start () {
		InitializeNodeNetwork (100);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void InitializeNodeNetwork(int numObstacles){
		
		var node = GameObject.Find ("Node");
		var obstacle = GameObject.Find ("Obstacle");
		var width = boardWidth;
		var height = boardHeight;

		GameObject goal = GameObject.Find ("Goal");
		Vector3 goalPos = goal.transform.localPosition;
		goalPos[1] = 0;

		walkablePositions.Add(new KeyValuePair<Vector3, bool>(goalPos, true));
		nodeReference.Add (new KeyValuePair<Vector3, GameObject> (goalPos, goal));

		List<Vector3> obstacles = GenerateObstacles (numObstacles);

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				Vector3 newPosition = new Vector3 (i, 0, j);
				GameObject copy;
				if (obstacles.Contains(newPosition)) {
					obstacle.GetComponent<SpriteRenderer>().sprite = obstacleSprites[Random.Range(0, obstacleSprites.Count - 1)];

					copy = Instantiate (obstacle);
					copy.transform.position = newPosition;

					if (!walkablePositions.ContainsKey(newPosition))
						walkablePositions.Add (new KeyValuePair<Vector3, bool> (newPosition, false));
				} else {
					copy = Instantiate (node);
					copy.transform.position = newPosition;

					if (!walkablePositions.ContainsKey(newPosition))
						walkablePositions.Add (new KeyValuePair<Vector3, bool> (newPosition, true));
				}

				if (!nodeReference.ContainsKey(newPosition))
					nodeReference.Add (newPosition, copy);
			}
		}
	}

	List<Vector3> GenerateObstacles(int num){

		GameObject goal = GameObject.Find ("Goal");
		Vector3 goalPos = goal.transform.localPosition;
		goalPos[1] = 0;

		List<Vector3> obstacles = new List<Vector3> ();

		for (int i = 0; i < num; i++) {
			Vector3 nodePosition = new Vector3 (Random.Range (0, boardWidth - 1), 0, Random.Range (0, boardHeight - 1));

			if (nodePosition != goalPos)
				obstacles.Add (nodePosition);
		}

		return obstacles;
	}
}
