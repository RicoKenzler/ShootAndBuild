using UnityEngine;

public class Movable : MonoBehaviour
{
	private float forceDegenerationPerSecond = 1;

	private Rigidbody rigid;

	void Awake()
	{
		rigid = GetComponent<Rigidbody>();
	}

	void Update()
	{
	}
	
}
