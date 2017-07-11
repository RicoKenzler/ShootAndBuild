using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAB
{

	public class QuestPanel : MonoBehaviour 
	{
        public Text winTextPanel;
		public Text loseTextPanel;
        Animator winTextAnimator;
		Animator loseTextAnimator;

		//-------------------------------------------------

		void Awake()
		{
			winTextAnimator		= winTextPanel.GetComponent<Animator>();
			loseTextAnimator	= loseTextPanel.GetComponent<Animator>();
		}

		//-------------------------------------------------
		
		void Start() 
		{
			
		}
		
		//-------------------------------------------------
		
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

            if (winTextPanel.text != winString)
            {
                winTextPanel.text = winString;
                winTextAnimator.SetTrigger("Grow");
            }

			if (loseTextPanel.text != loseString)
            {
                loseTextPanel.text = loseString;
                loseTextAnimator.SetTrigger("Grow");
            }
		}
	}

}