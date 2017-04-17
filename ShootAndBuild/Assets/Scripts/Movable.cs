using UnityEngine;

public class Movable : MonoBehaviour
{
	private float forceDegenerationPerSecond = 12;

	private Rigidbody rigid;

	void Awake()
	{
		rigid = GetComponent<Rigidbody>();

		moveForce = new Vector3();
		impulseForce = new Vector3();
	}

	void Update()
	{
		rigid.velocity = moveForce + impulseForce;

		float delta = forceDegenerationPerSecond * Time.deltaTime;
		float max = Mathf.Max(impulseForce.magnitude - delta, 0.0f);
		impulseForce = Vector3.ClampMagnitude(impulseForce, max);
	}
	
	public Vector3 moveForce
	{
		get; set;
	}

	public Vector3 impulseForce
	{
		get; set;
	}
}
