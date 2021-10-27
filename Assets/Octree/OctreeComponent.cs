using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeComponent : MonoBehaviour
{
	public Vector3 dimensions = new Vector3(512, 512, 512);
	public uint capacity = 8;

	private Vector3 point;
	private Vector3 starting_range;
	private Vector3 ending_range;
	[SerializeField]
	private GameObject octantGeneratorPrefab;
	[SerializeField]
	private GameObject pointGeneratorPrefab;
	
	private Octree<int> octree;

	private IList<GameObject> prefabOctantInstances;
	private IList<GameObject> prefabPointInstances;

	public string x_coordinate {
		set { point.x = float.Parse(value); }
	}
	public string y_coordinate {
		set { point.y = float.Parse(value); }
	}
	public string z_coordinate {
		set { point.z = float.Parse(value); }
	}

	public string x_query_starting_range {
		set { starting_range.x = float.Parse(value); }
	}
	public string y_query_starting_range {
		set { starting_range.y = float.Parse(value); }
	}
	public string z_query_starting_range {
		set { starting_range.z = float.Parse(value); }
	}

	public string x_query_ending_range {
		set { ending_range.x = float.Parse(value); }
	}
	public string y_query_ending_range {
		set { ending_range.y = float.Parse(value); }
	}
	public string z_query_ending_range {
		set { ending_range.z = float.Parse(value); }
	}

	private bool highlighted_points = false;
	private float highlight_timer = 0.0f;

	// Start is called before the first frame update
	void Start()
	{
		point = new Vector3(0, 0, 0);
		octree = new Octree<int>(this.transform.position, dimensions, capacity);
		prefabOctantInstances = new List<GameObject>();
		prefabPointInstances = new List<GameObject>();

		DrawTree();
	}

	// Update is called once per frame
	void Update()
	{
		//Debug.Log(prefabInstances.Count + " instances in Octree");
		if (!highlighted_points)
			return;

		if (highlighted_points)
			highlight_timer += Time.deltaTime;

		if (highlighted_points && highlight_timer > 5.0f) {

			foreach (var point in prefabPointInstances) {
				if (point == null)
					continue;
				Renderer point_renderer = point.GetComponent<Renderer>();
				point_renderer.material.color = Color.white;
            }

			highlighted_points = false;
			highlight_timer = 0.0f;
        }
	}

	public void InsertPoint() {
		octree.Insert(point);
		foreach (var instance in prefabOctantInstances)
			Destroy(instance);
		foreach (var instance in prefabPointInstances)
			Destroy(instance);
		DrawTree();
		Debug.Log("Inserted point at " + point);
	}

	public void InsertRandomPoint() {
		point.x = Random.Range(-dimensions.x, dimensions.x);
		point.y = Random.Range(-dimensions.y, dimensions.y);
		point.z = Random.Range(-dimensions.z, dimensions.z);
		InsertPoint();
    }

	private void DrawTree() {
		DrawNode(octree.Root);
	}

	public void SearchInOctant() {
		IList<Vector3> points_found = octree.Search(starting_range, ending_range);

		Debug.Log("Requested from: " + starting_range + " to " + ending_range);

		GameObject query_range = GameObject.Instantiate(octantGeneratorPrefab) as GameObject;
		LineRenderer drawable_query_range = query_range.GetComponent<LineRenderer>();

		drawable_query_range.startWidth = 0.25f;
		drawable_query_range.positionCount = 16;

		Vector3[] vertexPoints =
		{
			new Vector3(starting_range.x, starting_range.y, starting_range.z),
			new Vector3(starting_range.x, starting_range.y, ending_range.z),
			new Vector3(starting_range.x, ending_range.y, ending_range.z),
			new Vector3(starting_range.x, ending_range.y, starting_range.z),
			new Vector3(starting_range.x, starting_range.y, starting_range.z),
			new Vector3(ending_range.x, starting_range.y, starting_range.z),
			new Vector3(ending_range.x, starting_range.y, ending_range.z),
			new Vector3(ending_range.x, ending_range.y, ending_range.z),
			new Vector3(ending_range.x, ending_range.y, starting_range.z),
			new Vector3(ending_range.x, starting_range.y, starting_range.z),
			new Vector3(ending_range.x, ending_range.y, starting_range.z),
			new Vector3(starting_range.x, ending_range.y, starting_range.z),
			new Vector3(starting_range.x, ending_range.y, ending_range.z),
			new Vector3(ending_range.x, ending_range.y, ending_range.z),
			new Vector3(ending_range.x, starting_range.y, ending_range.z),
			new Vector3(starting_range.x, starting_range.y, ending_range.z)
		};

		drawable_query_range.SetPositions(vertexPoints);
		drawable_query_range.startColor = Color.red;
		drawable_query_range.endColor = Color.red;

		foreach (var point in prefabPointInstances) {
			if (point == null)
				continue;
			foreach (var point_found in points_found) {
				if (point.transform.position == point_found) {
					Renderer point_renderer = point.GetComponent<Renderer>();
					point_renderer.material.color = Color.red;
                }
            }
        }
		highlighted_points = true;

		Destroy(drawable_query_range, 5.0f);
	}

	private void DrawNode(Octree<int>.OctreeNode<int> node) {
		Vector3 nodeCenter = node.NodeOctant.Center;
		Vector3 nodeRange = node.NodeOctant.Dimensions;
		IList<Vector3> points = node.Points;
		Octree<int>.OctreeNode<int>[] children = node.Children;

		prefabOctantInstances.Add(GameObject.Instantiate(octantGeneratorPrefab) as GameObject);
		LineRenderer drawableOctant = prefabOctantInstances[prefabOctantInstances.Count - 1].GetComponent<LineRenderer>();

		drawableOctant.startWidth = 0.25f;
		drawableOctant.positionCount = 16;

		Vector3[] vertexPoints =
		{
			new Vector3(nodeCenter.x - nodeRange.x, nodeCenter.y - nodeRange.y, nodeCenter.z - nodeRange.z),
			new Vector3(nodeCenter.x - nodeRange.x, nodeCenter.y - nodeRange.y, nodeCenter.z + nodeRange.z),
			new Vector3(nodeCenter.x - nodeRange.x, nodeCenter.y + nodeRange.y, nodeCenter.z + nodeRange.z),
			new Vector3(nodeCenter.x - nodeRange.x, nodeCenter.y + nodeRange.y, nodeCenter.z - nodeRange.z),
			new Vector3(nodeCenter.x - nodeRange.x, nodeCenter.y - nodeRange.y, nodeCenter.z - nodeRange.z),
			new Vector3(nodeCenter.x + nodeRange.x, nodeCenter.y - nodeRange.y, nodeCenter.z - nodeRange.z),
			new Vector3(nodeCenter.x + nodeRange.x, nodeCenter.y - nodeRange.y, nodeCenter.z + nodeRange.z),
			new Vector3(nodeCenter.x + nodeRange.x, nodeCenter.y + nodeRange.y, nodeCenter.z + nodeRange.z),
			new Vector3(nodeCenter.x + nodeRange.x, nodeCenter.y + nodeRange.y, nodeCenter.z - nodeRange.z),
			new Vector3(nodeCenter.x + nodeRange.x, nodeCenter.y - nodeRange.y, nodeCenter.z - nodeRange.z),
			new Vector3(nodeCenter.x + nodeRange.x, nodeCenter.y + nodeRange.y, nodeCenter.z - nodeRange.z),
			new Vector3(nodeCenter.x - nodeRange.x, nodeCenter.y + nodeRange.y, nodeCenter.z - nodeRange.z),
			new Vector3(nodeCenter.x - nodeRange.x, nodeCenter.y + nodeRange.y, nodeCenter.z + nodeRange.z),
			new Vector3(nodeCenter.x + nodeRange.x, nodeCenter.y + nodeRange.y, nodeCenter.z + nodeRange.z),
			new Vector3(nodeCenter.x + nodeRange.x, nodeCenter.y - nodeRange.y, nodeCenter.z + nodeRange.z),
			new Vector3(nodeCenter.x - nodeRange.x, nodeCenter.y - nodeRange.y, nodeCenter.z + nodeRange.z)
		};

		drawableOctant.SetPositions(vertexPoints);

		foreach (var point in points) {
			prefabPointInstances.Add(GameObject.Instantiate(pointGeneratorPrefab) as GameObject);
			prefabPointInstances[prefabPointInstances.Count - 1].transform.position = point;
		}

		if (node.Divided)
			foreach (var child in children)
				DrawNode(child);
	}
}
