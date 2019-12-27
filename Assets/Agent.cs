using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {

	IDictionary<Vector3, Vector3> nodeParents = new Dictionary<Vector3, Vector3>();
	IDictionary<Vector3, Sprite> prevSprite = new Dictionary<Vector3, Sprite> ();
	public IDictionary<Vector3, bool> walkablePositions;

	public int IDDFSLimit = 50;

	NodeNetworkCreator nodeNetwork;
	IList<Vector3> path;

	bool moveCube = false;
	int i;

	bool solutionVisible;
	string lastSearchType;

	// Use this for initialization
	void Start () {
		nodeNetwork = GameObject.Find ("NodeNetwork").GetComponent<NodeNetworkCreator> ();
		walkablePositions = nodeNetwork.walkablePositions;
	}
	
	// Update is called once per frame
	void Update () {
		if (moveCube) {
			float step = Time.deltaTime * 5;
			transform.position = Vector3.MoveTowards (transform.position, path[i], step);
			if (transform.position.Equals (path [i]) && i >= 0)
				i--;
			if (i < 0)
				moveCube = false;
		}
	}

	Vector3 FindShortestPathBFS(Vector3 startPosition, Vector3 goalPosition){
        uint nodeVisitCount = 0;
        float timeNow = Time.realtimeSinceStartup;

		Queue<Vector3> queue = new Queue<Vector3> ();
		HashSet<Vector3> exploredNodes = new HashSet<Vector3> ();
		queue.Enqueue (startPosition);

		while (queue.Count != 0) {
			Vector3 currentNode = queue.Dequeue ();
            nodeVisitCount++;

			if (samePosition(currentNode, goalPosition)) {
                print("BFS time: " + (Time.realtimeSinceStartup - timeNow).ToString());
                print(string.Format("BFS visits: {0} ({1:F2}%)", nodeVisitCount, (nodeVisitCount / (double)walkablePositions.Count) * 100));

                return currentNode;
			}

			IList<Vector3> nodes = GetWalkableNodes (currentNode);

			foreach(Vector3 node in nodes){
				if(!exploredNodes.Contains(node)) {
					//Mark the node as explored
					exploredNodes.Add(node);

					//Store a reference to the previous node
					nodeParents.Add (node, currentNode);

					//Add this to the queue of nodes to examine
					queue.Enqueue (node);
				}
			}
		}

		return startPosition;
	}

	Vector3 FindShortestPathIDDFS(Vector3 startPosition, Vector3 goalPosition, int limit)
	{
		Vector3 result = startPosition;

		for (int i = 2; i <= limit; i ++)
		{
			print("new iteration: " + limit);
			nodeParents = new Dictionary<Vector3, Vector3>();
			result = IDDFS(startPosition, goalPosition, i);

			if (!samePosition(result, startPosition))
				break;
		}

		return result;
	}

	Vector3 IDDFS(Vector3 startPosition, Vector3 goalPosition, int limit){
        uint nodeVisitCount = 0;
        float timeNow = Time.realtimeSinceStartup;

		Stack<Vector3> stack = new Stack<Vector3> ();
		HashSet<Vector3> exploredNodes = new HashSet<Vector3> ();
		stack.Push (startPosition);

		while (stack.Count != 0) {
			Vector3 currentNode = stack.Pop ();
            nodeVisitCount++;

			if (samePosition(currentNode, goalPosition)) {
                print("IDDFS time: " + (Time.realtimeSinceStartup - timeNow).ToString());
                print(string.Format("DFS visits: {0} ({1:F2}%)", nodeVisitCount, (nodeVisitCount / (double)walkablePositions.Count) * 100));

				return currentNode;
			}

			// IDDFS Limit:
			if (stack.Count >= limit)
				continue;

			IList<Vector3> nodes = GetWalkableNodes (currentNode);

			foreach(Vector3 node in nodes){
				if(!exploredNodes.Contains(node)) {
					//Mark the node as explored
					exploredNodes.Add(node);

					//Store a reference to the previous node
					nodeParents.Add (node, currentNode);

					//Add this to the queue of nodes to examine
					stack.Push (node);
				}
			}
		}

		return startPosition;
	}

	Vector3 FindShortestPathDFS(Vector3 startPosition, Vector3 goalPosition){
        uint nodeVisitCount = 0;
        float timeNow = Time.realtimeSinceStartup;

		Stack<Vector3> stack = new Stack<Vector3> ();
		HashSet<Vector3> exploredNodes = new HashSet<Vector3> ();
		stack.Push (startPosition);

		while (stack.Count != 0) {
			Vector3 currentNode = stack.Pop ();
            nodeVisitCount++;

			if (samePosition(currentNode, goalPosition)) {
                print("DFS time: " + (Time.realtimeSinceStartup - timeNow).ToString());
                print(string.Format("DFS visits: {0} ({1:F2}%)", nodeVisitCount, (nodeVisitCount / (double)walkablePositions.Count) * 100));

				return currentNode;
			}

			IList<Vector3> nodes = GetWalkableNodes (currentNode);

			foreach(Vector3 node in nodes){
				if(!exploredNodes.Contains(node)) {
					//Mark the node as explored
					exploredNodes.Add(node);

					//Store a reference to the previous node
					nodeParents.Add (node, currentNode);

					//Add this to the queue of nodes to examine
					stack.Push (node);
				}
			}
		}

		return startPosition;
	}

	bool CanMove(Vector3 nextPosition) {
		return (walkablePositions.ContainsKey (nextPosition) ? walkablePositions [nextPosition] : false);
	}

	public void DisplayShortestPath(string searchType) {
		if (solutionVisible && searchType == lastSearchType) {
			foreach (Vector3 node in path) {
				// Don't replace the Goal sprite
				if (samePosition(node, GameObject.Find ("Goal").transform.localPosition))
					continue;

				nodeNetwork.nodeReference [node].GetComponent<SpriteRenderer> ().sprite = prevSprite[node];
			}

			solutionVisible = false;
			return;
		}
		
		nodeParents = new Dictionary<Vector3, Vector3>();
		path = FindShortestPath (searchType);

		Sprite bfsTile = Resources.Load<Sprite>("road_5");
		Sprite dfsTile = Resources.Load<Sprite> ("road_16");
		Sprite iddfsTile = Resources.Load<Sprite>("road_17");

		foreach (Vector3 node in path) {
			// Don't replace the Goal sprite
			if (samePosition(node, GameObject.Find ("Goal").transform.localPosition))
				continue;

			prevSprite[node] = nodeNetwork.nodeReference [node].GetComponent<SpriteRenderer> ().sprite;

			if (searchType == "IDDFS") {
				nodeNetwork.nodeReference [node].GetComponent<SpriteRenderer> ().sprite = iddfsTile;
			} else if (searchType == "DFS") {
				nodeNetwork.nodeReference [node].GetComponent<SpriteRenderer> ().sprite = dfsTile;
			} else {
				nodeNetwork.nodeReference [node].GetComponent<SpriteRenderer> ().sprite = bfsTile;
			}
		}

		i = path.Count - 1;

		solutionVisible = true;
		lastSearchType = searchType;
	}

	public void MoveCube(){
		moveCube = true;
	}

	IList<Vector3> FindShortestPath(string searchType){

		IList<Vector3> path = new List<Vector3> ();
		Vector3 goal = Vector3.zero;
		if (searchType == "IDDFS") {
			goal = FindShortestPathIDDFS (this.transform.localPosition, GameObject.Find ("Goal").transform.localPosition, IDDFSLimit);
		} else if (searchType == "DFS") {
			goal = FindShortestPathDFS (this.transform.localPosition, GameObject.Find ("Goal").transform.localPosition);
		} else if (searchType == "BFS") {
			goal = FindShortestPathBFS (this.transform.localPosition, GameObject.Find ("Goal").transform.localPosition);
		}

		if (goal == this.transform.localPosition) {
			//No solution was found.
			return null;
		}

		Vector3 curr = goal;
		while (curr != this.transform.localPosition) {
			path.Add (curr);
			curr = nodeParents [curr];
		}

		return path;
	}

	IList<Vector3> GetWalkableNodes(Vector3 curr) {

		IList<Vector3> walkableNodes = new List<Vector3> ();

		IList<Vector3> possibleNodes = new List<Vector3> () {
			new Vector3 (curr.x + 1, curr.y, curr.z),
			new Vector3 (curr.x - 1, curr.y, curr.z),
			new Vector3 (curr.x, curr.y, curr.z + 1),
			new Vector3 (curr.x, curr.y, curr.z - 1),
            // new Vector3 (curr.x + 1, curr.y, curr.z + 1),
            // new Vector3 (curr.x + 1, curr.y, curr.z - 1),
            // new Vector3 (curr.x - 1, curr.y, curr.z + 1),
            // new Vector3 (curr.x - 1, curr.y, curr.z - 1)
        };

		foreach (Vector3 node in possibleNodes) {
			if (CanMove (node)) {
				walkableNodes.Add (node);
			} 
		}

		return walkableNodes;
	}


	bool samePosition(Vector3 first, Vector3 second)
	{
		return first[0] == second[0] && first[2] == second[2];
	}
}
