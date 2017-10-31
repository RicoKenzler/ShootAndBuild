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

			switch (GameManager.instance.winCondition)
            {
                case WinCondition.MoneyTotal:
                    winString += "Gather Money\n";
                    winString += GameManager.instance.GetCurrentWinConditionContext() + " / " + GameManager.instance.winConditionContextValue + "\n\n";
                    break;
            }

			string loseString = "";

            switch (GameManager.instance.loseCondition)
            {
                case LoseCondition.Default:
                    // nothing special to draw
                    break;
                case LoseCondition.DestroyObject:
                    if (GameManager.instance.loseConditionContextObject)
                    {
                        Attackable attackable = GameManager.instance.loseConditionContextObject.GetComponent<Attackable>();

                        loseString += GameManager.instance.loseConditionContextObject.name + "\n";
                        loseString += attackable.health + " / " + attackable.maxHealth + " HP";
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