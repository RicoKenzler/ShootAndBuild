using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAB
{ 
    public enum GameStatus
    {
        Running,
        Paused,

        Lost,
        Won
    }
  
	///////////////////////////////////////////////////////////////////////////
	  
    public enum WinCondition
    {
        MoneyTotal          // gain X money
    };

	///////////////////////////////////////////////////////////////////////////

    public enum LoseCondition
    {
        Default,             // lose all lifes
        DestroyObject,
    };

	///////////////////////////////////////////////////////////////////////////

    public class GameManager : MonoBehaviour
    {
        public WinCondition winCondition    = WinCondition.MoneyTotal;
        public LoseCondition loseCondition  = LoseCondition.DestroyObject;

        public int winConditionContextValue = 1000;
        public GameObject winConditionContextObject = null;
        public int loseConditionContextValue = 0;
        public GameObject loseConditionContextObject = null;

        public RectTransform winText;
        public RectTransform loseText;

        public AudioData winSound;
        public AudioData loseSound;

        public Canvas canvas;

        private GameStatus gameStatus = GameStatus.Running;

        public GameStatus Status
        {
            get {  return this.gameStatus; }
        }

        public static GameManager Instance
        {
            get; private set;
        }

        // Use this for initialization
        void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (gameStatus == GameStatus.Running)
            {
                CheckWinConditions();
            }

            if (gameStatus == GameStatus.Running)
            {
                CheckLoseConditions();
            }

            if (gameStatus == GameStatus.Running || gameStatus == GameStatus.Paused)
            {
                if (InputManager.instance.DidAnyPlayerJustPress(ButtonType.Start))
                {
                    TogglePause();
                }
            }

			if (InputManager.instance.DidAnyPlayerJustPress(ButtonType.Restart))
			{
				Time.timeScale = 1.0f;
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
        }

        void CheckWinConditions()
        {
            Debug.Assert(gameStatus == GameStatus.Running);

            switch (winCondition)
            {
                case WinCondition.MoneyTotal:
                    if (GetCurrentWinConditionContext() >= winConditionContextValue)
                    {
                        WinGame();
                    }
                    break;
            }
        }

        public int GetCurrentWinConditionContext()
        {
            switch (winCondition)
            {
                case WinCondition.MoneyTotal:
                    return Inventory.sharedInventoryInstance.GetItemCount(ItemType.Gold);
            }

            return -1;
        }

        public int GetCurrentLoseConditionContext()
        {
            switch (loseCondition)
            {
                case LoseCondition.Default:
                    return -1;

                case LoseCondition.DestroyObject:
                    if (loseConditionContextObject)
                    {
                        Attackable loseConditionAttackable = loseConditionContextObject.GetComponent<Attackable>();
                        return loseConditionAttackable.Health;
                    }
                    else
                    {
                        return 0;
                    }
            }

            return -1;
        }

        void CheckLoseConditions()
        {
            Debug.Assert(gameStatus == GameStatus.Running);

            bool lostGame = false;

            switch (loseCondition)
            {
                case LoseCondition.Default:
                // we need to check this special condition anyways for all lose conditions
                break;

                case LoseCondition.DestroyObject:
                int context = GetCurrentLoseConditionContext();
                if (context <= 0)
                {
                    lostGame = true;
                }
                break;
            }

            // Default lose condition
            if (!lostGame)
            {
                if (Inventory.sharedInventoryInstance.GetItemCount(ItemType.ExtraLifes) <= 0)
                {
                    if (PlayerManager.instance.allAlivePlayers.Count == 0)
                    {
                        lostGame = true;
                    }
                }
            }
            
            if (lostGame)
            {
                LoseGame();
            }
        }

        void TogglePause()
        {
            Debug.Assert(gameStatus == GameStatus.Running || gameStatus == GameStatus.Paused);

            if (gameStatus == GameStatus.Running)
            {
                Time.timeScale = 0.0f;
                gameStatus = GameStatus.Paused;
            }
            else
            {
                Time.timeScale = 1.0f;
                gameStatus = GameStatus.Running;
            }

            CameraController.Instance.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = (gameStatus == GameStatus.Paused);
        }

        void WinGame()
        {
            if (CheatManager.instance.disableWin)
            {
                return;
            }

            AudioManager.instance.PlayAudio(winSound);
            Instantiate(winText.gameObject, canvas.transform, false);
            gameStatus = GameStatus.Won;
        }

        void LoseGame()
        {
            if (CheatManager.instance.disableLose)
            {
                return;
            }

            AudioManager.instance.PlayAudio(loseSound);
            Instantiate(loseText.gameObject, canvas.transform, false);
            gameStatus = GameStatus.Lost;
        }

    }

}