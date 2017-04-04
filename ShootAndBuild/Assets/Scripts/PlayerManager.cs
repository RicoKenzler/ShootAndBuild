using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public enum AxisType
{
    LeftAxisH,
    LeftAxisV,

    RightAxisH,
    RightAxisV,
}

public enum TriggerType
{
	LeftTrigger,
	RightTrigger
}

public enum ButtonType
{
    RightBumper,
	Taunt,
	Build
}

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

    enum InputMethod
    {
        Keyboard,

        Gamepad1,
        Gamepad2,
        Gamepad3,
        Gamepad4,
    }

	[SerializeField]
	InputMethod debugKeyboardEmulates;

    class Player
    {
        public GameObject playerObject;
        public InputMethod inputMethod;
        public bool isAlive;

		private float vibrationAmountL	= 0.0f;
		private float vibrationAmountR	= 0.0f;
		private float vibrateUntil		= 0.0f;
		private bool sleep				= true;

		public void StartVibration(float amountL, float amountR, float duration)
		{
			if (amountL <= vibrationAmountL && amountR <= vibrationAmountR)
			{
				// dont overwrite hard vibration with soft vibration
				return;
			}

			vibrationAmountL	= amountL;
			vibrationAmountR	= amountR;
			vibrateUntil		= Time.unscaledTime + duration;
			sleep				= false;
		}

		private int GetXInputPlayerIndex()
		{
			switch (inputMethod)
			{
				case InputMethod.Gamepad1: return 0;
				case InputMethod.Gamepad2: return 1;
				case InputMethod.Gamepad3: return 2;
				case InputMethod.Gamepad4: return 3;
			}

			return -1;
		}

		public void UpdateVibration()
		{
			if (sleep)
			{
				return;
			} 

			if (Time.unscaledTime > vibrateUntil)
			{
				// We vibrated long enough
				vibrationAmountL	= 0.0f;
				vibrationAmountR	= 0.0f;
				sleep = true;
			}

			int xInputIndex = GetXInputPlayerIndex();

			if (xInputIndex == -1)
			{
				// Player has no vibrating device
				return;
			}

			GamePad.SetVibration((PlayerIndex)xInputIndex, vibrationAmountL, vibrationAmountR);
		}
    }

    private Dictionary<PlayerID, Player> activePlayersById			= new Dictionary<PlayerID, Player>();
    private Dictionary<InputMethod, PlayerID> inputMethodToPlayerID = new Dictionary<InputMethod, PlayerID>();

    void Awake()
    {
        instance = this;
        allPlayers = new List<GameObject>();
    }

    void Update()
    {
        // always listen to spawn-button-presses
        TrySpawnNewPlayers();
        TryRespawnDeadPlayers();

		UpdateVibrations();
    }

	private void UpdateVibrations()
	{
		foreach(KeyValuePair<PlayerID, Player> player in activePlayersById)
		{
			player.Value.UpdateVibration();
		} 
	}

    private void TrySpawnNewPlayers()
    {
        foreach (InputMethod inputMethod in System.Enum.GetValues(typeof(InputMethod)))
        {
            if (inputMethodToPlayerID.ContainsKey(inputMethod))
            {
                // this controller already controls a player
                continue;
            }

            if (WasButtonJustPressed(inputMethod, ButtonType.RightBumper))
            {
                // 1) Try spawn NEW players
                foreach (PlayerID playerID in System.Enum.GetValues(typeof(PlayerID)))
                {
                    if (activePlayersById.ContainsKey(playerID))
                    {
                        // Player already exists
                        continue;
                    }

                    inputMethodToPlayerID[inputMethod] = playerID;
                    SpawnNewPlayer(playerID, inputMethod);

                    // enought spawning for this input method
                    break;
                }
            }
        }
    }

    private void TryRespawnDeadPlayers()
    {
        foreach (InputMethod inputMethod in System.Enum.GetValues(typeof(InputMethod)))
        {
            PlayerID playerID;

            if (!inputMethodToPlayerID.TryGetValue(inputMethod, out playerID))
            {
                continue;
            }

            Player player;
            if (!activePlayersById.TryGetValue(playerID, out player))
            {
                Debug.Assert(false, "InputMethod " + inputMethod + " registered to invalid player " + playerID);
                continue;
            }

            if (player.isAlive)
            {
                continue;
            }

            if (WasButtonJustPressed(inputMethod, ButtonType.RightBumper))
            {
                RespawnDeadPlayer(player);
            }
        }
    }

    private void RespawnDeadPlayer(Player player)
    {
        player.isAlive = true;
        player.playerObject.SetActive(true);
        player.playerObject.GetComponent<Attackable>().OnRespawn();

		player.StartVibration(0.5f, 0.5f, 0.2f);
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

		player.StartVibration(1.0f, 1.0f, 0.8f);
    }

	public GameObject GetNearestPlayer(Vector3 position)
	{
		GameObject bestPlayer = null;
		float bestDistanceSq = float.MaxValue;

		foreach(KeyValuePair<PlayerID, Player> player in activePlayersById)
		{
			if (!player.Value.isAlive)
			{
				continue;
			}

			GameObject playerObject = player.Value.playerObject;
			float distanceSq = (playerObject.transform.position - position).sqrMagnitude;

			if (distanceSq < bestDistanceSq)
			{
				bestDistanceSq = distanceSq;
				bestPlayer = playerObject;
			}
		}

		return bestPlayer;
	}

    private void SpawnNewPlayer(PlayerID playerID, InputMethod inputMethod)
    {
        GameObject newPlayerObject = Instantiate(playerPrefab, gameObject.transform);
		newPlayerObject.name = playerID.ToString();

        float randRadius = 5.0f;
        newPlayerObject.transform.position = new Vector3(Random.Range(-randRadius, randRadius), 0.0f, Random.Range(-randRadius, randRadius));
        newPlayerObject.GetComponent<InputController>().playerID = playerID;
        newPlayerObject.GetComponent<Attackable>().PlayerDies += OnPlayerDies;

        Player newPlayer = new Player();
        newPlayer.playerObject = newPlayerObject;
        newPlayer.inputMethod = inputMethod;
        newPlayer.isAlive = true;

        activePlayersById[playerID] = newPlayer;
        allPlayers.Add(newPlayerObject);

		newPlayer.StartVibration(0.5f, 0.5f, 0.2f);

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

    private InputMethod GetInputMethod(PlayerID playerID)
    {
        return GetPlayer(playerID).inputMethod;
    }

    private string InputMethodToPostfix(InputMethod inputMethod)
    {
		// for debugging other players: switch keyboard with keyboard emulation
		if (inputMethod == debugKeyboardEmulates)
		{
			inputMethod = InputMethod.Keyboard;
		}
		else if (inputMethod == InputMethod.Keyboard)
		{
			inputMethod = debugKeyboardEmulates;
		}

        switch (inputMethod)
        {
            case InputMethod.Keyboard:
                return " Keyboard";
            case InputMethod.Gamepad1:
                return " P1";
            case InputMethod.Gamepad2:
                return " P2";
            case InputMethod.Gamepad3:
                return " P3";
            case InputMethod.Gamepad4:
                return " P4";
        }

        return " InvalidInputMethod";
    }

    private string AxisToPrefix(AxisType axisType)
    {
        switch (axisType)
        {
            case AxisType.LeftAxisH:
                return "Left Horizontal";
            case AxisType.LeftAxisV:
                return "Left Vertical";
            case AxisType.RightAxisH:
                return "Right Horizontal";
            case AxisType.RightAxisV:
                return "Right Vertical";
        }

        return "InvalidAxis";
    }

	private string TriggerToPrefix(TriggerType triggerType)
	{
		switch (triggerType)
		{
			case TriggerType.LeftTrigger:
				return "Left Trigger";
			case TriggerType.RightTrigger:
				return "Right Trigger";
		}

		return "Invalid Trigger";
	}

    private string ButtonToPrefix(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.RightBumper:
                return "Right Bumper";
			case ButtonType.Taunt:
				return "Taunt";
			case ButtonType.Build:
				return "Unused";
        }

        return "InvalidButton ";
    }

    private float GetAxisValue(InputMethod inputMethod, AxisType axisType)
    {
        string inputName = AxisToPrefix(axisType) + InputMethodToPostfix(inputMethod);

        return Input.GetAxis(inputName);
    }

    public float GetAxisValue(PlayerID playerID, AxisType axisType)
    {
        InputMethod inputMethod = GetInputMethod(playerID);

        return GetAxisValue(inputMethod, axisType);
    }

    private bool IsButtonDown(InputMethod inputMethod, ButtonType buttonType)
    {
        string inputName = ButtonToPrefix(buttonType) + InputMethodToPostfix(inputMethod);

        return Input.GetButton(inputName);
    }

	private bool WasButtonJustPressed(InputMethod inputMethod, ButtonType buttonType)
	{
		string inputName = ButtonToPrefix(buttonType) + InputMethodToPostfix(inputMethod);

        return Input.GetButtonDown(inputName);
	}

	private float GetTriggerValue(InputMethod inputMethod, TriggerType triggerType)
	{
		string inputName = TriggerToPrefix(triggerType) + InputMethodToPostfix(inputMethod);

        return Input.GetAxis(inputName);
	}

	public bool IsTriggerPulled(PlayerID playerID, TriggerType triggerType)
	{
		InputMethod inputMethod = GetInputMethod(playerID);

        return (GetTriggerValue(inputMethod, triggerType) > 0.25f);
	}

	public float GetTriggerValue(PlayerID playerID, TriggerType triggerType)
    {
        InputMethod inputMethod = GetInputMethod(playerID);

        return GetTriggerValue(inputMethod, triggerType);
    }

    public bool IsButtonDown(PlayerID playerID, ButtonType buttonType)
    {
        InputMethod inputMethod = GetInputMethod(playerID);

        return IsButtonDown(inputMethod, buttonType);
    }

	public bool WasButtonJustPressed(PlayerID playerID, ButtonType buttonType)
	{
		InputMethod inputMethod = GetInputMethod(playerID);

        return WasButtonJustPressed(inputMethod, buttonType);
	}

	public void SetVibration(PlayerID playerID, float amountLeft, float amountRight, float duration)
	{
		GetPlayer(playerID).StartVibration(amountLeft, amountRight, duration);
	}

    public List<GameObject> allPlayers
    {
        get; private set;
    }

    public static PlayerManager instance
    {
        get; private set;
    }
}
