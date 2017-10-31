using UnityEngine;
using UnityEngine.UI;

namespace SAB
{

    public class XpBarPanel : MonoBehaviour
    {
        [SerializeField] private UiBar m_XpBar;

		///////////////////////////////////////////////////////////////////////////

		private Animator m_XpBarAnimator;

		///////////////////////////////////////////////////////////////////////////

		private void Awake()
		{
			m_XpBarAnimator = GetComponent<Animator>();
		}

		///////////////////////////////////////////////////////////////////////////
	
	    // Update is called once per frame
	    void Update ()
        {
            int killedEnemies = CounterManager.instance.GetCounterValue(null, CounterType.KilledEnemies).CurrentCount;
            float percentage = ((killedEnemies % 20) / 20.0f);
            
			if (percentage < m_XpBar.targetPercentage)
			{
				m_XpBarAnimator.SetTrigger("Grow");
			}

		    m_XpBar.targetPercentage = percentage;
	    }
    }

}