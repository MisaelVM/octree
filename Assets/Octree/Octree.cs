using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OctantID
{
	LeftBottomBack, // 000
	LeftBottomFront, // 001
	LeftTopBack, // 010
	LeftTopFront, // 011
	RightBottomBack, // 100
	RightBottomFront, // 101
	RightTopBack, // 110
	RightTopFront, // 111
}

public class Octree <T> {
	private OctreeNode<T> root;
	private int depth;

	public Octree(Vector3 center, Vector3 dimensions, uint capacity) {
		root = new OctreeNode<T>(center, dimensions, capacity);
	}


	public class OctreeNode <T> {
		private Octant octant;

		public class Octant {
			private Vector3 center;
			private float half_length;
			private float half_height;
			private float half_width;

			public Vector3 Center {
				get { return center; }
				set { center = value; }
			}

			public Vector3 Dimensions {
				get { return new Vector3(half_length, half_height, half_width); }
				set { half_length = value.x; half_height = value.y; half_width = value.z; }
			}

			public Octant(Vector3 center, float l, float h, float w) {
				this.center = center;
				half_length = l;
				half_height = h;
				half_width = w;
			}

			public Octant(Vector3 starting_range, Vector3 ending_range) {
				half_length = (ending_range.x - starting_range.x) / 2;
				half_height = (ending_range.y - starting_range.y) / 2;
				half_width = (ending_range.z - starting_range.z) / 2;

				center.x = starting_range.x + half_length;
				center.y = starting_range.y + half_height;
				center.z = starting_range.z + half_width;
			}

			public bool Contains(Vector3 point) {
				return (point.x >= center.x - half_length && point.x <= center.x + half_length &&
						point.y >= center.y - half_height && point.y <= center.y + half_height &&
						point.z >= center.z - half_width && point.z <= center.z + half_width);
			}

			public bool Intersects(Octant range) {
				return !(range.center.x - range.half_length > center.x + half_length || range.center.x + range.half_length < center.x - half_length ||
						range.center.x - range.half_height > center.y + half_height || range.center.y + range.half_height < center.y - half_height ||
						range.center.z - range.half_width > center.z + half_width || range.center.z + range.half_width < center.z - half_width);
			}

			public Vector3 Subdivide(int i) {
				Vector3 newCenter = center;

				newCenter.x += ((i & 4) == 4) ? half_length / 2.0f : -half_length / 2.0f;
				newCenter.y += ((i & 2) == 2) ? half_height / 2.0f : -half_height / 2.0f;
				newCenter.z += ((i & 1) == 1) ? half_width / 2.0f : -half_width / 2.0f;

				return newCenter;
			}
		}

		private IList<Vector3> points;
		private OctreeNode<T>[] children;
		private uint capacity;
		private bool divided;

		public OctreeNode(Vector3 center, Vector3 dimensions, uint capacity) {
			octant = new Octant(center, dimensions.x, dimensions.y, dimensions.z);
			points = new List<Vector3>();
			children = new OctreeNode<T>[8];
			this.capacity = capacity;
			divided = false;
		}

		public void Subdivide() {
			for (int i = 0; i < 8; ++i) {
				Vector3 newCenter = octant.Subdivide(i);
				Vector3 newDimensions = octant.Dimensions / 2.0f;
				children[i] = new OctreeNode<T>(newCenter, newDimensions, capacity);
			}
			divided = true;
		}

		public void Insert(Vector3 point) {
			if (!octant.Contains(point))
				return;

			if (points.Count < capacity) {
				points.Add(point);
				return;
			}

			if (!divided)
				Subdivide();

			for (int i = 0; i < 8; ++i)
				children[i].Insert(point);
		}

		public IList<Vector3> Search(Vector3 starting_range, Vector3 ending_range) {
			IList<Vector3> found = new List<Vector3>();
			Octant range = new Octant(starting_range, ending_range);
			if (!octant.Intersects(range))
				return found;
			else
				foreach (var point in points)
					if (range.Contains(point))
						found.Add(point);

			if (divided) {
				for (int i = 0; i < 8; ++i) {
					IList<Vector3> foundInChild = children[i].Search(starting_range, ending_range);
					foreach (var point in foundInChild)
						found.Add(point);
                }
            }

			return found;
		}

		public Octant NodeOctant
		{
			get { return octant; }
		}

		public IList<Vector3> Points
		{
			get { return points; }
		}

		public OctreeNode<T>[] Children
		{
			get { return children; }
		}

		public bool Divided
		{
			get { return divided; }
		}
	}

	public OctreeNode<T> Root
	{
		get { return root; }
	}

	public void Insert(Vector3 point)
	{
		root.Insert(point);
	}

	public IList<Vector3> Search(Vector3 starting_range, Vector3 ending_range)
    {
		return root.Search(starting_range, ending_range);
    }
}
