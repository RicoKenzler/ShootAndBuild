using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAB
{

	public class QuestPanel : MonoBehaviour 
	{
        [SerializeField] private Text m_WinTextPanel;
		[SerializeField] private Text m_LoseTextPanel;
        private Animator m_WinTextAnimator;
		private Animator m_LoseTextAnimator;

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			m_WinTextAnimator	= m_WinTextPanel.GetComponent<Animator>();
			m_LoseTextAnimator	= m_LoseTextPanel.GetComponent<Animator>();
		}
				
		///////////////////////////////////////////////////////////////////////////
		
		void Update() 
		{
            string winString = "";

			switch (GameManager.Instance.winCondition)
            {
                case WinCondition.MoneyTotal:
                    winString += "Gather Money\n";
                    winString += GameManager.Instance.GetCurrentWinConditionContext() + " / " + GameManager.Instance.winConditionContextValue + "\n\n";
                    break;
            }

			string loseString = "";

            switch (GameManager.Instance.loseCondition)
            {
                case LoseCondition.Default:
                    // nothing special to draw
                    break;
                case LoseCondition.DestroyObject:
                    if (GameManager.Instance.loseConditionContextObject)
                    {
                        Attackable attackable = GameManager.Instance.loseConditionContextObject.GetComponent<Attackable>();

                        loseString += GameManager.Instance.loseConditionContextObject.name + "\n";
                        loseString += attackable.Health + " / " + attackable.maxHealth + " HP";
                    }
                    else
                    {
                        loseString += "OBJECT DESTROYED";
                    }
                    break;
            }

            if (m_WinTextPanel.text != winString)
            {
                m_WinTextPanel.text = winString;
                m_WinTextAnimator.SetTrigger("Grow");
            }

			if (m_LoseTextPanel.text != loseString)
            {
                m_LoseTextPanel.text = loseString;
                m_LoseTextAnimator.SetTrigger("Grow");
            }
		}
	}

}