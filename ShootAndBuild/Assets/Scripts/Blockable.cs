using UnityEngine;

public class Blockable : MonoBehaviour
{
	private static Vector3 invalidPosition = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
	private Vector3 lastPosition = invalidPosition;
	private Vector3 lastGridPosition = invalidPosition;


	void Update()
	{
		Vector3 currentGridPosition = Grid.instance.Round(transform.position);
		if (currentGridPosition == lastGridPosition)
		{
			// we are still on the same field. Return!
			return;
		}

		if (lastPosition != invalidPosition)
		{
			Grid.instance.Free(gameObject, lastPosition);
		}

		lastGridPosition = currentGridPosition;
		lastPosition = transform.position;
		Grid.instance.Reserve(gameObject, transform.position);
	}
	
	// Frees the position if the Gameobjects dies, or gets disabled
	void OnDisable()
	{
		if (lastPosition != invalidPosition)
		{
			Grid.instance.Free(gameObject, lastPosition);
		}
	}
}
