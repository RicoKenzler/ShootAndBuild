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
        [SerializeField] private WinCondition	m_WinCondition	= WinCondition.MoneyTotal;
        [SerializeField] private LoseCondition	m_LoseCondition	= LoseCondition.DestroyObject;

        [SerializeField] private int			m_WinConditionContextValue		= 1000;
        [SerializeField] private GameObject		m_WinConditionContextObject		= null;
        [SerializeField] private int			m_LoseConditionContextValue		= 0;
        [SerializeField] private GameObject		m_LoseConditionContextObject	= null;

        [SerializeField] private RectTransform	m_WinText;
        [SerializeField] private RectTransform	m_LoseText;

        [SerializeField] private AudioData		m_WinSound;
        [SerializeField] private AudioData		m_LoseSound;

        private GameStatus m_GameStatus = GameStatus.Running;
        private Canvas m_Canvas;

		///////////////////////////////////////////////////////////////////////////

		public LoseCondition	loseCondition				{ get { return m_LoseCondition; } }
		public GameObject		loseConditionContextObject	{ get { return m_LoseConditionContextObject; } set { Debug.Assert(!m_LoseConditionContextObject); m_LoseConditionContextObject = value; } }
		public int				loseConditionContextValue	{ get { return m_LoseConditionContextValue; } }
		public WinCondition		winCondition				{ get { return m_WinCondition; } }
		public GameObject		winConditionContextObject	{ get { return m_WinConditionContextObject; } }
		public int				winConditionContextValue	{ get { return m_WinConditionContextValue; } }

		///////////////////////////////////////////////////////////////////////////

        public GameStatus Status { get {  return this.m_GameStatus; }  }
        public static GameManager instance	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;
        }

		///////////////////////////////////////////////////////////////////////////

		void Start()
		{
			m_Canvas = Canvas2D.instance.GetComponent<Canvas>();
		}

		///////////////////////////////////////////////////////////////////////////

        void Update()
        {
            if (m_GameStatus == GameStatus.Running)
            {
                CheckWinConditions();
            }

            if (m_GameStatus == GameStatus.Running)
            {
                CheckLoseConditions();
            }

            if (m_GameStatus == GameStatus.Running || m_GameStatus == GameStatus.Paused)
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

		///////////////////////////////////////////////////////////////////////////

        void CheckWinConditions()
        {
            Debug.Assert(m_GameStatus == GameStatus.Running);

            switch (m_WinCondition)
            {
                case WinCondition.MoneyTotal:
                    if (GetCurrentWinConditionContext() >= m_WinConditionContextValue)
                    {
                        WinGame();
                    }
                    break;
            }
        }

		///////////////////////////////////////////////////////////////////////////

        public int GetCurrentWinConditionContext()
        {
            switch (m_WinCondition)
            {
                case WinCondition.MoneyTotal:
                    return Inventory.sharedInventoryInstance.GetItemCount(ItemType.Gold);
            }

            return -1;
        }

		///////////////////////////////////////////////////////////////////////////

        public int GetCurrentLoseConditionContext()
        {
            switch (m_LoseCondition)
            {
                case LoseCondition.Default:
                    return -1;

                case LoseCondition.DestroyObject:
                    if (m_LoseConditionContextObject)
                    {
                        Attackable loseConditionAttackable = m_LoseConditionContextObject.GetComponent<Attackable>();
                        return loseConditionAttackable.health;
                    }
                    else
                    {
                        return 0;
                    }
            }

            return -1;
        }

		///////////////////////////////////////////////////////////////////////////

        void CheckLoseConditions()
        {
            Debug.Assert(m_GameStatus == GameStatus.Running);

            bool lostGame = false;

            switch (m_LoseCondition)
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

		///////////////////////////////////////////////////////////////////////////

        void TogglePause()
        {
            Debug.Assert(m_GameStatus == GameStatus.Running || m_GameStatus == GameStatus.Paused);

            if (m_GameStatus == GameStatus.Running)
            {
                Time.timeScale = 0.0f;
                m_GameStatus = GameStatus.Paused;
            }
            else
            {
                Time.timeScale = 1.0f;
                m_GameStatus = GameStatus.Running;
            }

            CameraController.instance.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = (m_GameStatus == GameStatus.Paused);
        }

		///////////////////////////////////////////////////////////////////////////

        void WinGame()
        {
            if (CheatManager.instance.disableWin)
            {
                return;
            }

            AudioManager.instance.PlayAudio(m_WinSound);
            Instantiate(m_WinText.gameObject, m_Canvas.transform, false);
            m_GameStatus = GameStatus.Won;
        }

		///////////////////////////////////////////////////////////////////////////

        void LoseGame()
        {
            if (CheatManager.instance.disableLose)
            {
                return;
            }

            AudioManager.instance.PlayAudio(m_LoseSound);
            Instantiate(m_LoseText.gameObject, m_Canvas.transform, false);
            m_GameStatus = GameStatus.Lost;
        }

    }

}