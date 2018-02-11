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

			float oldVelocityY = m_Rigidbody.velocity.y;
			
			const float VELOCITY_Y_THRESHOLD = 5.0f;
			if (oldVelocityY > VELOCITY_Y_THRESHOLD)
			{
				// Currently impulses can be very unpredictable so just stop y-impulses when they have too much impact.
				oldVelocityY = VELOCITY_Y_THRESHOLD;
				impulseForce = impulseForce.xz().To3D(0.0f);
			}

			m_Rigidbody.velocity = move + (impulseForce / m_Rigidbody.mass) + new Vector3(0.0f, oldVelocityY, 0.0f);

			float impulseMagnitudeDecrement = FORCE_DEGENERATION_PER_SECOND * Time.deltaTime;
			float newImpulseMagnitude = Mathf.Max(impulseForce.magnitude - impulseMagnitudeDecrement, 0.0f);
			impulseForce = Vector3.ClampMagnitude(impulseForce, newImpulseMagnitude);

			// fix object outside of grid
			Vector2 terrainSize = TerrainManager.instance.terrainSizeWS;
			bool xTooSmall = gameObject.transform.position.x < 0;
			bool zTooSmall = gameObject.transform.position.z < 0;
			bool xTooLarge = gameObject.transform.position.x >= terrainSize.x;
			bool zTooLarge = gameObject.transform.position.z >= terrainSize.y;

			if (xTooSmall || zTooSmall || xTooLarge || zTooLarge)
			{
				Debug.Log("<" + gameObject.name + "> outside playable area at " + gameObject.transform.position + ". Pushing towards inside.");
				
				Vector3 newPosition = gameObject.transform.position;
				const float PUSH_LENGTH = 5.0f;

				if (xTooSmall)
				{
					newPosition.x = PUSH_LENGTH;
				}
				else if (xTooLarge)
				{
					newPosition.x = terrainSize.x - PUSH_LENGTH;
				}

				if (zTooSmall)
				{
					newPosition.z = PUSH_LENGTH;
				}
				else if (zTooLarge)
				{
					newPosition.z = terrainSize.y - PUSH_LENGTH;
				}

				gameObject.transform.position = newPosition;

				m_Rigidbody.velocity = new Vector3(0,0,0);
			}

			// fix objects falling through terrain
			float terrainHeight = TerrainManager.instance.GetInterpolatedHeight(transform.position.x, transform.position.z);
			if (gameObject.transform.position.y < (terrainHeight - 1.0f))
			{
				Vector3 newPosition = gameObject.transform.position;
				Debug.Log("<" + gameObject.name + "> clipped below terrain at " + newPosition + ". Offsetting to new height.");
				newPosition.y = terrainHeight + 1.0f;
				gameObject.transform.position = newPosition;
				m_Rigidbody.velocity = new Vector3(0,0,0);
			}
		}
	
		///////////////////////////////////////////////////////////////////////////

	}

}