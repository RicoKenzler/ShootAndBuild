using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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

        // Use this for initialization
        void Start()
        {
            BuildingManager.instance.RegisterBuilding(this, false);

            this.m_Attackable = this.GetComponent<Attackable>();
            this.m_ChildsRenderer = this.GetComponentInChildren<Renderer>();

            if (this.m_Attackable == null)
            {
                Debug.LogWarning("Building without attackable");
            }
            this.m_Attackable.OnDamage += OnDamage;
        }

        ///////////////////////////////////////////////////////////////////////////

        void OnDisable()
        {
            BuildingManager.instance.RegisterBuilding(this, true);
        }

        ///////////////////////////////////////////////////////////////////////////

        private void OnDamage()
        {
            foreach (var m in m_ChildsRenderer.materials)
            {
                m.color = Color.Lerp(new Color(0.5f, 0.0f, 0.0f), Color.white, m_Attackable.healthNormalized);
            }
        }
    }
}