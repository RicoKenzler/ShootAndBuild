﻿using System.Collections.Generic;
using UnityEngine;

public enum PlayerID
{
    Player1,
    Player2,
    Player3,
    Player4
}

public delegate void PlayerHandler(PlayerID id);

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;
	public AudioClip[] spawnFailSounds;

	private ButtonType spawnButton = ButtonType.Taunt;

    class Player
    {
        public GameObject playerObject;
        public bool isAlive;
    }

    private Dictionary<PlayerID, Player> activePlayersById = new Dictionary<PlayerID, Player>();

    void Awake()
    {
        instance = this;
        allAlivePlayers = new List<GameObject>();
    }

    void Update()
    {
        // always listen to spawn-button-presses
        TrySpawnNewPlayers();
        TryRespawnDeadPlayers();
    }

    private void TrySpawnNewPlayers()
    {
		InputMethod? buttonPresser = InputManager.instance.IsButtonDownForUnusedInputMethod(spawnButton);
		
		if (!buttonPresser.HasValue)
		{
			return;
		}

        // 1) Try spawn NEW players
        foreach (PlayerID playerID in System.Enum.GetValues(typeof(PlayerID)))
        {
            if (activePlayersById.ContainsKey(playerID))
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

	private bool TrySpendLife()
	{
		if (Inventory.sharedInventoryInstance.GetItemCount(ItemType.ExtraLifes) > 0)
		{
			Inventory.sharedInventoryInstance.AddItem(ItemType.ExtraLifes, -1);
			return true;
		}

		GlobalPanel.instance.HighlightLifes();
		AudioManager.instance.PlayRandomOneShot2D(spawnFailSounds);
		return false;
	}

    private void TryRespawnDeadPlayers()
    {
		foreach (KeyValuePair<PlayerID, Player> playerPair in activePlayersById)
		{
			if (playerPair.Value.isAlive)
			{
				continue;
			}

			if (InputManager.instance.WasButtonJustPressed(playerPair.Key, spawnButton))
			{
				if (!TrySpendLife())
				{
					break;
				}

				RespawnDeadPlayer(playerPair.Key);
			}
		}
    }

    private void RespawnDeadPlayer(PlayerID playerID)
    {
		Player player = GetPlayer(playerID);

        player.isAlive = true;
        player.playerObject.SetActive(true);
        player.playerObject.GetComponent<Attackable>().OnRespawn();

		InputManager.instance.SetVibration(playerID, 0.5f, 0.5f, 0.2f);

		allAlivePlayers.Add(player.playerObject);
    }

    private void OnPlayerDies(PlayerID playerID)
    {
        Player player;
        if (!activePlayersById.TryGetValue(playerID, out player))
        {
            Debug.Assert(false, "Deleting player " + playerID + "that was not registered.");
            return;
        }

        Debug.Assert(player.isAlive, "Player " + playerID + " is dying though he is already dead");

        player.isAlive = false;
        player.playerObject.SetActive(false);

		InputManager.instance.SetVibration(playerID, 1.0f, 1.0f, 0.8f);

		bool removed = allAlivePlayers.Remove(player.playerObject);

		Debug.Assert(removed);
    }

    private void SpawnNewPlayer(PlayerID playerID)
    {
        GameObject newPlayerObject = Instantiate(playerPrefab, gameObject.transform);
		newPlayerObject.name = playerID.ToString();

        float randRadius = 5.0f;
        newPlayerObject.transform.position = new Vector3(Random.Range(-randRadius, randRadius), 0.0f, Random.Range(-randRadius, randRadius));
        newPlayerObject.GetComponent<InputController>().playerID = playerID;
        newPlayerObject.GetComponent<Attackable>().PlayerDies += OnPlayerDies;

        Player newPlayer = new Player();
        newPlayer.playerObject = newPlayerObject;
        newPlayer.isAlive = true;

        activePlayersById[playerID] = newPlayer;
        allAlivePlayers.Add(newPlayerObject);

		InputManager.instance.SetVibration(playerID, 0.5f, 0.5f, 0.2f);

		PlayerPanelGroup.instance.AddPlayerPanel(playerID, newPlayerObject);
    }

    private Player GetPlayer(PlayerID playerID)
    {
        if (!activePlayersById.ContainsKey(playerID))
        {
            Debug.Log("Accessing invalid Player " + playerID);
            return new Player();
        }

        return activePlayersById[playerID];
    }

    public List<GameObject> allAlivePlayers
    {
        get; private set;
    }

    public static PlayerManager instance
    {
        get; private set;
    }
}
