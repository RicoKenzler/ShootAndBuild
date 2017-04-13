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

		RectTransform newPanelRect = newPlayerPanelObject.GetComponent<RectTransform>();
		
		//newPanelRect.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		PlayerPanel newPlayerPanel = newPlayerPanelObject.GetComponent<PlayerPanel>();
		newPlayerPanel.AssignPlayer(player);

		playerPanels.Add(playerID, newPlayerPanel);

		//LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
		//LayoutRebuilder.MarkLayoutForRebuild(newPlayerPanel.transform as RectTransform);

		//newPlayerPanel.GetComponent<PlayerPanelResizer>().aspectRatio = playerPanelAspectRatio;
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
