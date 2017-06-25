using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAB
{

	public class QuestPanel : MonoBehaviour 
	{
        public Text questTextPanel;
        public Animator questTextAnimator;

		//-------------------------------------------------

		void Awake()
		{
			questTextAnimator = questTextPanel.GetComponent<Animator>();
		}

		//-------------------------------------------------
		
		void Start() 
		{
			
		}
		
		//-------------------------------------------------
		
		void Update() 
		{
            string finalString = "";

			switch (GameManager.Instance.winCondition)
            {
                case WinCondition.MoneyTotal:
                    finalString += "Gather Money\n";
                    finalString += GameManager.Instance.GetCurrentWinConditionContext() + " / " + GameManager.Instance.winConditionContextValue + "\n\n";
                    break;
            }

            switch (GameManager.Instance.loseCondition)
            {
                case LoseCondition.Default:
                    // nothing special to draw
                    break;
                case LoseCondition.DestroyObject:
                    if (GameManager.Instance.loseConditionContextObject)
                    {
                        Attackable attackable = GameManager.Instance.loseConditionContextObject.GetComponent<Attackable>();

                        finalString += GameManager.Instance.loseConditionContextObject.name + "\n";
                        finalString += attackable.Health + " / " + attackable.maxHealth + " HP";
                    }
                    else
                    {
                        finalString += "OBJECT DESTROYED";
                    }
                    break;
            }

            if (questTextPanel.text != finalString)
            {
                questTextPanel.text = finalString;
                HighlightQuest();
            }
		}

        public void HighlightQuest()
        {
            questTextAnimator.SetTrigger("Grow");
        }
	}

}