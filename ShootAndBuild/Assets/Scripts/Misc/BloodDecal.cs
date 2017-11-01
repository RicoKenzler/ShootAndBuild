using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public class BloodDecal : MonoBehaviour
	{
		private static Queue<BloodDecal> s_Decals = new Queue<BloodDecal>();
		private static int s_decalNum = 0;
		static GameObject s_DecalsContainer_IfAny = null;
		const string DECALS_CONTAINER_NAME = "Decals";

		private const int MAX_DECAL_COUNT = 1000;
		private const float DECAL_FADE_TIME = 1.0f;

		///////////////////////////////////////////////////////////////////////////

		private float m_Progress	= 0.0f;
		private bool m_FadeOut		= false;
		private bool m_FadeIn		= false;

		///////////////////////////////////////////////////////////////////////////

		void Start()
		{
			while (s_Decals.Count > MAX_DECAL_COUNT)
			{
				BloodDecal decal = s_Decals.Dequeue();
				decal.Vanish();
			}

			s_Decals.Enqueue(this);

			m_FadeIn = true;

			transform.localScale = Vector3.zero;

			Renderer renderer = GetComponentInChildren<Renderer>();
			float x = Random.Range(1, 3) * 0.5f;
			float y = Random.Range(1, 3) * 0.5f;
			renderer.material.mainTextureOffset = new Vector2(x, y);
		
			Vector3 pos = transform.localPosition;
			pos.y = 0.05f + 0.000001f * s_decalNum;
			transform.localPosition = pos;

			transform.Rotate(Vector3.up, Random.Range(0.0f, 360.0f));

			s_decalNum = (s_decalNum + 1) % MAX_DECAL_COUNT;
		}

		///////////////////////////////////////////////////////////////////////////

		void Update()
		{
			if (m_FadeIn)
			{
				m_Progress += Time.deltaTime / DECAL_FADE_TIME;
				m_Progress = Mathf.Clamp01(m_Progress);
				transform.localScale = Vector3.one * m_Progress;

				if (m_Progress == 1.0f)
				{
					m_FadeIn = false;
				}
			}

			if (m_FadeOut)
			{
				m_Progress -= Time.deltaTime / DECAL_FADE_TIME;
				m_Progress = Mathf.Clamp01(m_Progress);
				transform.localScale = Vector3.one * m_Progress;

				if (m_Progress == 0.0f)
				{
					Destroy(gameObject);
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////

		private void Vanish()
		{
			m_FadeIn = false;
			m_FadeOut = true;
		}

		///////////////////////////////////////////////////////////////////////////

		public static GameObject GetOrCreateDecalsContainer()
		{
			if (!s_DecalsContainer_IfAny)
			{
				s_DecalsContainer_IfAny = new GameObject();
				s_DecalsContainer_IfAny.name = DECALS_CONTAINER_NAME;
			}

			return s_DecalsContainer_IfAny;
		}
	}

}