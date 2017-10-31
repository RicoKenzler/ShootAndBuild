using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


namespace SAB
{

    public class PlayerPanelGroup : MonoBehaviour
    {
        [SerializeField] private PlayerPanel m_PlayerPanelPrefab;

		///////////////////////////////////////////////////////////////////////////

        private Dictionary<PlayerID, PlayerPanel> m_PlayerPanels = new Dictionary<PlayerID, PlayerPanel>();

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;
        }

		///////////////////////////////////////////////////////////////////////////

        public void AddPlayerPanel(PlayerID playerID, GameObject player)
        {
            if (GetPlayerPanel(playerID) != null)
            {
                Debug.LogWarning("Creating player UI for " + playerID + "twice?");
                return;
            }

            GameObject newPlayerPanelObject = Instantiate(m_PlayerPanelPrefab.gameObject, gameObject.transform, false);
            newPlayerPanelObject.name = "Player Panel " + playerID;

            PlayerPanel newPlayerPanel = newPlayerPanelObject.GetComponent<PlayerPanel>();
            newPlayerPanel.AssignPlayer(player);

            m_PlayerPanels.Add(playerID, newPlayerPanel);
        }

		///////////////////////////////////////////////////////////////////////////

        public PlayerPanel GetPlayerPanel(PlayerID playerID)
        {
            PlayerPanel outPanel;
            m_PlayerPanels.TryGetValue(playerID, out outPanel);

            return outPanel;
        }

		///////////////////////////////////////////////////////////////////////////

        public static PlayerPanelGroup instance
        {
            get; private set;
        }
    }
}