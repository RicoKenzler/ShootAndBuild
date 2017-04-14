using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerPanelGroup : MonoBehaviour
{
	[SerializeField]
	private PlayerPanel playerPanelPrefab;

	private Dictionary<PlayerID, PlayerPanel> playerPanels = new Dictionary<PlayerID, PlayerPanel>();

	void Awake()
	{
		instance = this;
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public void AddPlayerPanel(PlayerID playerID, GameObject player)
	{
		if (GetPlayerPanel(playerID) != null)
		{
			Debug.LogWarning("Creating player UI for " + playerID + "twice?");
			return;
		}

		GameObject newPlayerPanelObject = Instantiate(playerPanelPrefab.gameObject, gameObject.transform, false);
		newPlayerPanelObject.name = "Player Panel " + playerID;

		PlayerPanel newPlayerPanel = newPlayerPanelObject.GetComponent<PlayerPanel>();
		newPlayerPanel.AssignPlayer(player);

		playerPanels.Add(playerID, newPlayerPanel);
	}

	public PlayerPanel GetPlayerPanel(PlayerID playerID)
	{
		PlayerPanel outPanel;
		playerPanels.TryGetValue(playerID, out outPanel);

		return outPanel;
	}

	public static PlayerPanelGroup instance
	{
		get; private set;
	}
}
