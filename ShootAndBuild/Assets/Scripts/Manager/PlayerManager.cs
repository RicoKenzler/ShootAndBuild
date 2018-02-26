using System.Collections.Generic;
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
		[SerializeField] private Color[]	m_playerColors;

		///////////////////////////////////////////////////////////////////////////

        private const ButtonType SPAWN_BUTTON = ButtonType.Taunt;

        private Dictionary<PlayerID, Player> m_ActivePlayersById = new Dictionary<PlayerID, Player>();

		///////////////////////////////////////////////////////////////////////////

		
        public List<InputController>	allAlivePlayers			{ get; private set; }
		public List<InputController>	allDeadOrAlivePlayers	{ get; private set; }
		public Color[]					playerColors			{ get { return m_playerColors; } }

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

		float m_HackedyHackLastSpawnNewPlayerTry = float.MinValue;

		///////////////////////////////////////////////////////////////////////////

        private void TrySpawnNewPlayers()
        {
            InputMethod? buttonPresser = InputManager.instance.IsButtonDownForUnusedInputMethod(SPAWN_BUTTON);

            if (!buttonPresser.HasValue)
            {
                return;
            }

			// This ugly hack is here because we do not have a "WasButtonJustPressedForUnusedInputMethod", resulting in
			// isdown, isdown, isdown spam, resulting in audio-cannot-spawn-spam
			if (Time.time - m_HackedyHackLastSpawnNewPlayerTry < 1.0f)
			{
				return;
			}
			m_HackedyHackLastSpawnNewPlayerTry = Time.time;

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
			StorableItemData extraLifeData = GameManager.instance.extraLifeItemData;

            if (Inventory.sharedInventoryInstance.GetItemCount(extraLifeData) > 0)
            {
                Inventory.sharedInventoryInstance.ChangeItemCount(extraLifeData, -1);
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
			// try to spawn that player 50 times at most
			const int maxTries = 50;
			int tries = 0;

			// the radius is increasing every 5 tries
			float minRadius = 5.0f;
			float maxRadius = minRadius + 3.0f * (tries / 5);

			int playerCount = allAlivePlayers.Count;
			Vector2 mapCenter = TerrainManager.instance.GetTerrainCenter2D();

			do
			{
				if (playerCount == 0)
				{
					// very first player, take a position near the center
					Vector2 offset = Random.insideUnitCircle * maxRadius;
					Vector2 pos2D = mapCenter + offset;
					Vector3 pos3D = pos2D.To3D(0.0f);
					bool isFree = BlockerGrid.instance.IsFree(m_PlayerPrefab, pos3D);
					if (isFree)
					{
						return pos3D;
					}
				}
				else
				{
					// spawn near a random other player
					int rndIndex = Random.Range(0, playerCount - 1);
					Vector3 playerPosition = allAlivePlayers[rndIndex].transform.position;
					float range = Random.Range(minRadius, maxRadius);
					Vector2 offset = Random.insideUnitCircle * range;
					Vector3 pos3D = playerPosition + offset.To3D(0.0f);
					bool isFree = BlockerGrid.instance.IsFree(m_PlayerPrefab, pos3D);
					if (isFree)
					{
						return pos3D;
					}
				}
			}
			while (++tries < maxTries);

			Debug.LogError("Could not find a valid player spawn position!");
			return mapCenter.To3D(0.0f);
		}

		///////////////////////////////////////////////////////////////////////////

        private void SpawnNewPlayer(PlayerID playerID)
        {
            GameObject newPlayerObject = Instantiate(m_PlayerPrefab, gameObject.transform);
            newPlayerObject.name = playerID.ToString();

            newPlayerObject.transform.position = GetRandomPlayerSpawnPosition();
            newPlayerObject.GetComponent<InputController>().playerID = playerID;
            newPlayerObject.GetComponent<Attackable>().PlayerDies += OnPlayerDies;
			newPlayerObject.GetComponent<Shooter>().ReceiveStartWeapons();

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

		///////////////////////////////////////////////////////////////////////////

		public void OnDrawGizmosSelected()
		{
			foreach (PlayerID playerID in System.Enum.GetValues(typeof(PlayerID)))
			{
				if (!HasPlayerJoined(playerID))
				{
					break;
				}

				Color color = playerColors[(int) playerID];
				InputController player = GetPlayer(playerID).playerObject.GetComponent<InputController>();

				if (color != player.playerColor)
				{
					// update color and force health bar update
					player.playerColor = color;
					Attackable attackable = player.GetComponent<Attackable>();
					if (attackable.health == attackable.maxHealth)
					{
						attackable.DealDamage(1, attackable.gameObject, attackable.gameObject);
					}
					else
					{
						attackable.Heal(1.0f);
					}
				}

			}
		}
    }
}
