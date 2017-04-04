using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerPanelGroup : MonoBehaviour
{
	[SerializeField]
	private PlayerPanel playerPanelPrefab;

	public  float playerPanelAspectRatio		= 1.35f;
	private float lastPlayerPanelAspectRatio	= 1.35f;

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
		if (lastPlayerPanelAspectRatio != playerPanelAspectRatio)
		{
			foreach (PlayerPanel panel in playerPanels.Values)
			{
				panel.GetComponent<PlayerPanelResizer>().aspectRatio = playerPanelAspectRatio;
			}

			LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
			lastPlayerPanelAspectRatio = playerPanelAspectRatio;
		}
	}

	public void AddPlayerPanel(PlayerID playerID, GameObject player)
	{
		if (GetPlayerPanel(playerID) != null)
		{
			Debug.LogWarning("Creating player UI for " + playerID + "twice?");
			return;
		}

		GameObject newPlayerPanelObject = Instantiate(playerPanelPrefab.gameObject, gameObject.transform);
		newPlayerPanelObject.name = "Player Panel " + playerID;

		RectTransform newPanelRect = newPlayerPanelObject.GetComponent<RectTransform>();
		
		newPanelRect.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		PlayerPanel newPlayerPanel = newPlayerPanelObject.GetComponent<PlayerPanel>();
		newPlayerPanel.AssignPlayer(player);

		playerPanels.Add(playerID, newPlayerPanel);

		LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
		LayoutRebuilder.MarkLayoutForRebuild(newPlayerPanel.transform as RectTransform);

		newPlayerPanel.GetComponent<PlayerPanelResizer>().aspectRatio = playerPanelAspectRatio;
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
