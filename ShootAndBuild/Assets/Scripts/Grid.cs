using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
	public float resolution = 0.5f;
	public int size = 10;

	private List<bool> grid = new List<bool>();
	private int width = 0;
	private int height = 0;
	private Vector3 center = new Vector3();
	

	void Awake()
	{
		instance = this;

		width = (int)(size / resolution);
		height = (int)(size / resolution);
		grid.Capacity = width * height;

		center.Set(width / 2, 0, height / 2);
		
		for (int i = 0; i < width * height; ++i)
		{
			grid.Add(false);
		}
	}

	void OnDrawGizmos()
	{
		float w = width;
		float h = height;
		Vector3 half = new Vector3(resolution * 0.5f, resolution * 0.5f, resolution * 0.5f);

		for (int i = 0; i < grid.Count; ++i)
		{
			float x = (i % w - w / 2.0f) * resolution;
			float y = ((int)(i / h) - h / 2.0f) * resolution;
			Vector3 drawPos = new Vector3(x, 5.0f, y);
			drawPos += half;

			Gizmos.color = grid[i] == true ? Color.red : Color.yellow;
			Gizmos.DrawCube(drawPos, Vector3.one * resolution * 0.8f);
		}
	}

	public void Reserve(GameObject go, Vector3 position)
	{
		Set(go, true, position);
	}

	public void Free(GameObject go, Vector3 position)
	{
		Set(go, false, position);
	}

	private void Set(GameObject go, bool value, Vector3 position)
	{
		Rect area = GetAffectedArea(go, position);

		for (int y = (int)area.yMin; y < area.yMax; ++y)
		{
			for (int x = (int)area.xMin; x < area.xMax; ++x)
			{
				int index = x + y * width;
				grid[index] = value;
			}
		}
	}

	public bool IsFree(GameObject go, Vector3 position)
	{
		Rect area = GetAffectedArea(go, position);

		for (int y = (int)area.yMin; y < area.yMax; ++y)
		{
			for (int x = (int)area.xMin; x < area.xMax; ++x)
			{
				int index = x + y * width;
				if (grid[index] == true)
				{
					return false;
				}
			}
		}

		return true;
	}

	private Rect GetAffectedArea(GameObject go, Vector3 position)
	{
		Collider collider = go.GetComponent<Collider>();
		Vector3 extents = collider.bounds.extents;

		// urgh, extends is zero. This can happen for unbuild objects
		// use size instead. Don't know if this is a good idea
		if (extents == Vector3.zero)
		{
			BoxCollider box = go.GetComponent<BoxCollider>();
			if (box)
			{
				extents = box.size / 2;
			}
			else
			{
				CapsuleCollider capsule = go.GetComponent<CapsuleCollider>();
				extents = capsule.radius * Vector3.one;
			}
		}

		position = Round(position);

		Vector3 min = (position - extents) / resolution + center;
		Vector3 max = (position + extents) / resolution + center;
		min = Round(min);
		max = Round(max);

		return new Rect(min.x, min.z, max.x - min.x, max.z - min.z);
	}

	public Vector3 Round(Vector3 input)
	{
		return new Vector3(Mathf.Round(input.x / resolution) * resolution, Mathf.Round(input.y / resolution) * resolution, Mathf.Round(input.z / resolution) * resolution);
	}

	public static Grid instance
	{
		get; private set;
	}

}
