using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
    public class HealthBarManager : MonoBehaviour
    {
        [SerializeField] private HealthBar m_HealthBarPrefab;

        private List<HealthBar> m_HealthBars = new List<HealthBar>();

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;
        }
		
		///////////////////////////////////////////////////////////////////////////

        public void AddHealthBar(Attackable attackable)
        {
            HealthBar instanceBG = Instantiate(m_HealthBarPrefab, transform);
            instanceBG.target = attackable;
            instanceBG.name = "HealthBarBG - " + attackable.name;
            instanceBG.isBackgroundDuplicate = true;
            m_HealthBars.Add(instanceBG.GetComponent<HealthBar>());

            HealthBar instance = Instantiate(m_HealthBarPrefab, transform);
            instance.target = attackable;
            instance.name = "HealthBar - " + attackable.name;
            m_HealthBars.Add(instance.GetComponent<HealthBar>());
        }
		
		///////////////////////////////////////////////////////////////////////////

        public void RemoveHealthBar(Attackable attackable)
        {
            for (int i = m_HealthBars.Count - 1; i >= 0; --i)
            {
                HealthBar bar = m_HealthBars[i];

                if (bar.target == attackable)
                {
                    m_HealthBars.RemoveAt(i);

                    // when the play mode stops, the bar may already be destroyed
                    if (bar == null)
                    {
                        continue;
                    }

                    Destroy(bar.gameObject);

                    // no return: currently we add a hacky background bar in addition
                }
            }
        }
		
		///////////////////////////////////////////////////////////////////////////

        public static HealthBarManager instance
        {
            get; private set;
        }
    }
}