using SAB;
using UnityEngine;

public class Movable : MonoBehaviour
{
	private const float	FORCE_DEGENERATION_PER_SECOND = 64.0f;

	private Rigidbody	m_Rigidbody;
	private Buffable	m_Buffable;

	///////////////////////////////////////////////////////////////////////////

	public Vector3 moveForce	{ get; set; }
	public Vector3 impulseForce	{ get; set; }

	///////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Buffable = GetComponent<Buffable>();

		moveForce = new Vector3();
		impulseForce = new Vector3();
	}

	///////////////////////////////////////////////////////////////////////////

	void Update()
	{
		Vector3 move = moveForce * (m_Buffable ? m_Buffable.GetSpeedMultiplier() : 1.0f);

		m_Rigidbody.velocity = move + impulseForce;

		float delta = FORCE_DEGENERATION_PER_SECOND * Time.deltaTime;
		float max = Mathf.Max(impulseForce.magnitude - delta, 0.0f);
		impulseForce = Vector3.ClampMagnitude(impulseForce, max);
	}
	
	///////////////////////////////////////////////////////////////////////////

}
