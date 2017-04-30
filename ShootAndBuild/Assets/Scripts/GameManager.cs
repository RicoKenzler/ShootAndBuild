using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public RectTransform winText;
	public RectTransform loseText;

	public AudioData winSound;
	public AudioData loseSound;

	public Canvas canvas;

	private GameStatus gameStatus = GameStatus.Running;

	enum GameStatus
	{
		Running,
		Paused,

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

		if (gameStatus == GameStatus.Running || gameStatus == GameStatus.Paused)
		{
			if (InputManager.instance.DidAnyPlayerJustPress(ButtonType.Start))
            {
				TogglePause();
			}
		}
	}

	void CheckWinConditions()
	{
		Debug.Assert(gameStatus == GameStatus.Running);

		if (Inventory.sharedInventoryInstance.GetItemCount(ItemType.Gold) >= 1000)
		{
			WinGame();
		}
	}

	void CheckLoseConditions()
	{
		Debug.Assert(gameStatus == GameStatus.Running);

		if (Inventory.sharedInventoryInstance.GetItemCount(ItemType.ExtraLifes) <= 0)
		{
			if (PlayerManager.instance.allAlivePlayers.Count == 0)
			{
				LoseGame();
			}
		}
	}

	void TogglePause()
	{
		Debug.Assert(gameStatus == GameStatus.Running || gameStatus == GameStatus.Paused);

		if (gameStatus == GameStatus.Running)
		{
			Time.timeScale = 0.0f;
			gameStatus = GameStatus.Paused;
		}
		else
		{
			Time.timeScale = 1.0f;
			gameStatus = GameStatus.Running;
		}

		CameraController.instance.GetComponent<UnityStandardAssets.ImageEffects.Blur>().enabled = (gameStatus == GameStatus.Paused);
	}

	void WinGame()
	{
		if (CheatManager.instance.disableWin)
		{
			return;
		}

		AudioManager.instance.PlayAudio(winSound);
		Instantiate(winText.gameObject, canvas.transform, false);
		gameStatus = GameStatus.Won;
	}

	void LoseGame()
	{
		if (CheatManager.instance.disableLose)
		{
			return;
		}

		AudioManager.instance.PlayAudio(loseSound);
		Instantiate(loseText.gameObject, canvas.transform, false);
		gameStatus = GameStatus.Lost;
	}
}
