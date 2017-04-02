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

		GameObject newPlayerPanelObject = Instantiate(playerPanelPrefab.gameObject, gameObject.transform);
		newPlayerPanelObject.name = "Player Panel " + playerID;

		RectTransform newPanelRect = newPlayerPanelObject.GetComponent<RectTransform>();
		
		newPanelRect.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		PlayerPanel newPlayerPanel = newPlayerPanelObject.GetComponent<PlayerPanel>();
		newPlayerPanel.AssignPlayer(player);

		playerPanels.Add(playerID, newPlayerPanel);

		UpdatePlayerPanelTransforms();
	}

	void UpdatePlayerPanelTransforms()
	{
		RectTransform prefabTransform = playerPanelPrefab.gameObject.GetComponent<RectTransform>();
		float relativePrefabWidth = (prefabTransform.anchorMax.x - prefabTransform.anchorMin.x);

		float border = relativePrefabWidth * 0.2f;

		int panelCount = playerPanels.Count;

		if (panelCount == 0)
		{
			return;
		}

		float totalSpaceNeeded = panelCount * relativePrefabWidth + border * (panelCount - 1);

		if (totalSpaceNeeded > 1)
		{
			Debug.LogWarning("Player panels do not fit");
			totalSpaceNeeded	= 1.0f;
			border				= 0.0f;
			relativePrefabWidth	= 1.0f / (float) panelCount;
		}

		float firstPanelStart = 0.5f - totalSpaceNeeded * 0.5f;

		int panelIndex = 0;

		foreach (PlayerPanel panel in playerPanels.Values)
		{
			float panelStart = firstPanelStart + (panelIndex * (relativePrefabWidth + border));

			RectTransform panelTransform = panel.GetComponent<RectTransform>();

			panelTransform.anchorMin = new Vector2(panelStart, 0.0f);
			panelTransform.anchorMax = new Vector2(panelStart + relativePrefabWidth, 1.0f);

			panelTransform.offsetMin = new Vector2(0.0f, 0.0f);
			panelTransform.offsetMax = new Vector2(0.0f, 0.0f);

			panelIndex++;
		}

		
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
