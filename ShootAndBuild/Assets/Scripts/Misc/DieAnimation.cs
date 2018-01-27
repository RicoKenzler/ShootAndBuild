using UnityEngine;

namespace SAB
{
	public class DieAnimation : MonoBehaviour
	{
		private const float SINK_DELAY = 5.0f;
		private const float SINK_DURATION = 5.0f;

		private float m_TimeSinceStart = 0.0f;
		private Vector3 m_StartPos;
		private Vector3 m_ObjectSize;
		private Collider m_ColliderRef;

		///////////////////////////////////////////////////////////////////////////

		void Start()
		{
			foreach (MonoBehaviour c in GetComponents<MonoBehaviour>())
			{
				if (c != this)
				{
					c.enabled = false;
				}
			}

			m_StartPos = transform.position;

			m_ColliderRef = GetComponent<Collider>();
			m_ObjectSize = m_ColliderRef.bounds.size;
			m_ColliderRef.enabled = false;

			GetComponent<Rigidbody>().velocity		= Vector3.zero;
			GetComponent<Rigidbody>().useGravity	= false;

			GetComponentInChildren<Animation>().Play("die");
		}

		///////////////////////////////////////////////////////////////////////////

		void Update()
		{
			m_TimeSinceStart += Time.deltaTime;

			if (m_TimeSinceStart > SINK_DELAY)
			{
				float p = (m_TimeSinceStart - SINK_DELAY) / SINK_DURATION;
				float offset = m_ObjectSize.y * p;
				Vector3 pos = m_StartPos;
				pos.y -= offset;
				transform.position = pos;
			}
			if (m_TimeSinceStart > SINK_DELAY + SINK_DURATION)
			{
				Destroy(gameObject);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void ShowBloodDecal(GameObject decal)
		{
			GameObject parent = BloodDecal.GetOrCreateDecalsContainer();

			GameObject instance = Instantiate(decal, parent.transform);
			Vector3 pos = transform.position;
			pos.y = 0;
			instance.transform.position = pos;
		}
	}
}
