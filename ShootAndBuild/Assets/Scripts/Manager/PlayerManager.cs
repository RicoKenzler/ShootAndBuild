﻿using System.Collections.Generic;
using UnityEngine;

namespace SAB
{

	///////////////////////////////////////////////////////////////////////////

    public enum PlayerID
    {
        Player1 = 0,
        Player2 = 1,
        Player3 = 2,
        Player4 = 3,

        Count = 4
    }

	///////////////////////////////////////////////////////////////////////////

    public class PlayerManager : MonoBehaviour
    {
        class Player
        {
            public GameObject playerObject;
            public bool isAlive;
        }

		///////////////////////////////////////////////////////////////////////////

        [SerializeField] private GameObject m_PlayerPrefab;
        [SerializeField] private AudioData	m_SpawnFailSound;

		///////////////////////////////////////////////////////////////////////////

        private const ButtonType SPAWN_BUTTON = ButtonType.Taunt;

        private Dictionary<PlayerID, Player> m_ActivePlayersById = new Dictionary<PlayerID, Player>();

		///////////////////////////////////////////////////////////////////////////

		
        public List<InputController> allAlivePlayers		{ get; private set; }
		public List<InputController> allDeadOrAlivePlayers	{ get; private set; }

        public static PlayerManager instance { get; private set; }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance				= this;
            allAlivePlayers			= new List<InputController>();
			allDeadOrAlivePlayers	= new List<InputController>();
        }

		///////////////////////////////////////////////////////////////////////////

        void Update()
        {
            // always listen to spawn-button-presses
            TrySpawnNewPlayers();
            TryRespawnDeadPlayers();
        }

		///////////////////////////////////////////////////////////////////////////

		public bool IsAlive(PlayerID playerID)
		{
			return GetPlayer(playerID).isAlive;
		}

		///////////////////////////////////////////////////////////////////////////

        private void TrySpawnNewPlayers()
        {
            InputMethod? buttonPresser = InputManager.instance.IsButtonDownForUnusedInputMethod(SPAWN_BUTTON);

            if (!buttonPresser.HasValue)
            {
                return;
            }

            // 1) Try spawn NEW players
            foreach (PlayerID playerID in System.Enum.GetValues(typeof(PlayerID)))
            {
                if (m_ActivePlayersById.ContainsKey(playerID))
                {
                    // Player already exists
                    continue;
                }

                if (!TrySpendLife())
                {
                    break;
                }

                InputManager.instance.OnSpawnNewPlayer(playerID, buttonPresser.Value);

                SpawnNewPlayer(playerID);

                // enought spawning for this input method
                break;
            }
        }

		///////////////////////////////////////////////////////////////////////////

        public bool HasPlayerJoined(PlayerID playerID)
        {
            return m_ActivePlayersById.ContainsKey(playerID);
        }

		///////////////////////////////////////////////////////////////////////////

        private bool TrySpendLife()
        {
            if (Inventory.sharedInventoryInstance.GetItemCount(ItemType.ExtraLifes) > 0)
            {
                Inventory.sharedInventoryInstance.AddItem(ItemType.ExtraLifes, -1);
                return true;
            }

            GlobalPanel.instance.HighlightLifes();
            AudioManager.instance.PlayAudio(m_SpawnFailSound);
            return false;
        }

		///////////////////////////////////////////////////////////////////////////

        private void TryRespawnDeadPlayers()
        {
            foreach (KeyValuePair<PlayerID, Player> playerPair in m_ActivePlayersById)
            {
                if (playerPair.Value.isAlive)
                {
                    continue;
                }

                if (InputManager.instance.WasButtonJustPressed(playerPair.Key, SPAWN_BUTTON))
                {
                    if (!TrySpendLife())
                    {
                        break;
                    }

                    RespawnDeadPlayer(playerPair.Key);
                }
            }
        }

		///////////////////////////////////////////////////////////////////////////

        private void RespawnDeadPlayer(PlayerID playerID)
        {
            Player player = GetPlayer(playerID);

            player.isAlive = true;
            player.playerObject.SetActive(true);
            player.playerObject.GetComponent<Attackable>().OnRespawn(GetRandomPlayerSpawnPosition());

            InputManager.instance.SetVibration(playerID, 0.5f, 0.5f, 0.2f);

            allAlivePlayers.Add(player.playerObject.GetComponent<InputController>());
        }

		///////////////////////////////////////////////////////////////////////////

        private void OnPlayerDies(PlayerID playerID)
        {
            Player player;
            if (!m_ActivePlayersById.TryGetValue(playerID, out player))
            {
                Debug.Assert(false, "Deleting player " + playerID + "that was not registered.");
                return;
            }

            Debug.Assert(player.isAlive, "Player " + playerID + " is dying though he is already dead");

            player.isAlive = false;
            player.playerObject.SetActive(false);

            InputManager.instance.SetVibration(playerID, 1.0f, 1.0f, 0.8f);

            bool removed = allAlivePlayers.Remove(player.playerObject.GetComponent<InputController>());

            Debug.Assert(removed);
        }

		///////////////////////////////////////////////////////////////////////////

		private Vector3 GetRandomPlayerSpawnPosition()
		{
			float randRadius = 5.0f;
			Vector2 spawnCircleCenter = TerrainManager.instance.GetTerrainCenter2D();
			Vector3 rndSpawnOffset = new Vector3(Random.Range(-randRadius, randRadius), 0.0f, Random.Range(-randRadius, randRadius));

            return new Vector3(spawnCircleCenter.x, 0.0f, spawnCircleCenter.y) + rndSpawnOffset;
		}

		///////////////////////////////////////////////////////////////////////////

        private void SpawnNewPlayer(PlayerID playerID)
        {
            GameObject newPlayerObject = Instantiate(m_PlayerPrefab, gameObject.transform);
            newPlayerObject.name = playerID.ToString();

            newPlayerObject.transform.position = GetRandomPlayerSpawnPosition();
            newPlayerObject.GetComponent<InputController>().playerID = playerID;
            newPlayerObject.GetComponent<Attackable>().PlayerDies += OnPlayerDies;

            Player newPlayer = new Player();
            newPlayer.playerObject = newPlayerObject;
            newPlayer.isAlive = true;

            m_ActivePlayersById[playerID] = newPlayer;
            allAlivePlayers.Add(newPlayerObject.GetComponent<InputController>());
			allDeadOrAlivePlayers.Add(newPlayerObject.GetComponent<InputController>());

            InputManager.instance.SetVibration(playerID, 0.5f, 0.5f, 0.2f);

            PlayerPanelGroup.instance.AddPlayerPanel(playerID, newPlayerObject);
        }

		///////////////////////////////////////////////////////////////////////////

        private Player GetPlayer(PlayerID playerID)
        {
            if (!m_ActivePlayersById.ContainsKey(playerID))
            {
                Debug.Log("Accessing invalid Player " + playerID);
                return new Player();
            }

            return m_ActivePlayersById[playerID];
        }
    }
}