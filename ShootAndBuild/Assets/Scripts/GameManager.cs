using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public RectTransform winText;
	public RectTransform loseText;

	public Canvas canvas;

	private GameStatus gameStatus = GameStatus.Running;

	enum GameStatus
	{
		Running,
		Lost,
		Won
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (gameStatus == GameStatus.Running)
		{
			CheckWinConditions();
		}

		if (gameStatus == GameStatus.Running)
		{
			CheckLoseConditions();
		}
	}

	void CheckWinConditions()
	{
		if (Inventory.sharedInventoryInstance.GetItemCount(ItemType.Gold) >= 1000)
		{
			WinGame();
		}
	}

	void CheckLoseConditions()
	{
		if (Inventory.sharedInventoryInstance.GetItemCount(ItemType.ExtraLifes) <= 0)
		{
			if (PlayerManager.instance.allAlivePlayers.Count == 0)
			{
				LoseGame();
			}
		}
	}

	void WinGame()
	{
		Instantiate(winText.gameObject, canvas.transform, false);
		gameStatus = GameStatus.Won;
	}

	void LoseGame()
	{
		Instantiate(loseText.gameObject, canvas.transform, false);
		gameStatus = GameStatus.Lost;
	}
}
