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
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void AddPlayerPanel(PlayerID playerID)
	{
		if (GetPlayerPanel(playerID) != null)
		{
			Debug.LogWarning("Creating player UI for " + playerID + "twice?");
			return;
		}

		GameObject newPlayerPanel = Instantiate(playerPanelPrefab.gameObject, gameObject.transform);
		RectTransform newPanelRect = newPlayerPanel.GetComponent<RectTransform>();

		
		newPanelRect.anchorMin = new Vector2(0.0f, 0.0f);
		newPanelRect.anchorMax = new Vector2(1.0f, 1.0f);
		newPanelRect.

		playerPanels.Add(playerID, newPlayerPanel.GetComponent<PlayerPanel>());
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
