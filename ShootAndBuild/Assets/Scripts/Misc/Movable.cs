using SAB;
using UnityEngine;

namespace SAB
{
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

			Vector3 oldVelocityY = new Vector3(0.0f, m_Rigidbody.velocity.y, 0.0f);
			
			m_Rigidbody.velocity = move + impulseForce + oldVelocityY;

			float impulseMagnitudeDecrement = FORCE_DEGENERATION_PER_SECOND * Time.deltaTime;
			float newImpulseMagnitude = Mathf.Max(impulseForce.magnitude - impulseMagnitudeDecrement, 0.0f);
			impulseForce = Vector3.ClampMagnitude(impulseForce, newImpulseMagnitude);
		}
	
		///////////////////////////////////////////////////////////////////////////

	}

}