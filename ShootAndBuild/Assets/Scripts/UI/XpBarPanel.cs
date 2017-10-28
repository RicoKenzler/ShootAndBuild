using UnityEngine;
using UnityEngine.UI;

namespace SAB
{

    public class XpBarPanel : MonoBehaviour
    {
        public UiBar XpBar;
		public Animator XpBarAnimator;

		private void Awake()
		{
			XpBarAnimator = GetComponent<Animator>();
		}

		// Use this for initialization
		void Start ()
        {
		
	    }
	
	    // Update is called once per frame
	    void Update ()
        {
            int killedEnemies = CounterManager.instance.GetCounterValue(null, CounterType.KilledEnemies).CurrentCount;
            float percentage = ((killedEnemies % 20) / 20.0f);
            
			if (percentage < XpBar.TargetPercentage)
			{
				XpBarAnimator.SetTrigger("Grow");
			}

		    XpBar.TargetPercentage = percentage;
	    }
    }

}