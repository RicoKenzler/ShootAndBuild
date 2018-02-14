﻿using UnityEngine;

namespace SAB
{
	///////////////////////////////////////////////////////////////////////////

	public class Building : MonoBehaviour
    {
        [SerializeField] ItemAndCount[]		m_Costs;

        private Attackable	m_Attackable;
        private Renderer	m_ChildsRenderer;

        ///////////////////////////////////////////////////////////////////////////

		public ItemAndCount[] costs { get { return m_Costs; }}

		///////////////////////////////////////////////////////////////////////////

        void Awake()
		{
			m_Attackable = GetComponent<Attackable>();
			m_ChildsRenderer = GetComponentInChildren<Renderer>();

			if (m_Attackable == null)
			{
				Debug.LogWarning("Building without attackable");
			}

			m_Attackable.OnDamage += OnDamage;
		}

		///////////////////////////////////////////////////////////////////////////
		
		void OnEnable()
		{
			BuildingManager.instance.RegisterBuilding(this, false);
		}

        ///////////////////////////////////////////////////////////////////////////

        void OnDisable()
        {
            BuildingManager.instance.RegisterBuilding(this, true);
        }

		///////////////////////////////////////////////////////////////////////////

		public void SetBuildPreview(bool enable)
		{
			if (enable)
			{
				Material material = GetComponentInChildren<MeshRenderer>().material;
				material.SetFloat("_Mode", 3.0f);
				material.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);

				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.EnableKeyword("_ALPHABLEND_ON");
				material.renderQueue = 3000;
			}
			else
			{
				Material material = GetComponentInChildren<MeshRenderer>().material;
				material.SetFloat("_Mode", 0.0f);
				material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			}

			EnableComponents(!enable);
		}

		///////////////////////////////////////////////////////////////////////////

		private void EnableComponents(bool enable)
		{
			MonoBehaviour[] behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();

			for (int i = 0; i < behaviours.Length; ++i)
			{
				MonoBehaviour behaviour = behaviours[i];
				if (behaviour != this)
				{
					behaviour.enabled = enable;
				}
			}

			Collider c = gameObject.GetComponent<Collider>();
			if (c)
			{
				c.enabled = enable;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		private void OnDamage()
        {
            foreach (var m in m_ChildsRenderer.materials)
            {
                m.color = Color.Lerp(new Color(0.5f, 0.0f, 0.0f), Color.white, m_Attackable.healthNormalized);
            }
        }

		///////////////////////////////////////////////////////////////////////////
	}
}