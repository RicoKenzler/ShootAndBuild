using SAB;
using UnityEngine;

public class Movable : MonoBehaviour
{
	private float forceDegenerationPerSecond = 64;

	private Rigidbody rigid;
	private Buffable buffable;

	void Awake()
	{
		rigid = GetComponent<Rigidbody>();
		buffable = GetComponent<Buffable>();

		moveForce = new Vector3();
		impulseForce = new Vector3();
	}

	void Update()
	{
		Vector3 move = moveForce * (buffable ? buffable.GetSpeedMultiplier() : 1.0f);

		rigid.velocity = move + impulseForce;

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
