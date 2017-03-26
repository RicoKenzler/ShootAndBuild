using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AxisType
{
    LeftAxisH,
    LeftAxisV,

    RightAxisH,
    RightAxisV,
}

public enum ButtonType
{
    Shoot,
}

public enum PlayerID
{
	Player1,
	Player2,
	Player3,
	Player4
}

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

    struct Player
    {
        public GameObject    playerObject;
        public InputMethod   inputMethod;
    }

    Dictionary<PlayerID,	Player>		activePlayersById		= new Dictionary<PlayerID, Player>();
	Dictionary<InputMethod, PlayerID>	inputMethodToPlayerID	= new Dictionary<InputMethod, PlayerID>();

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
		// always listen to spawn-button-presses
		TrySpawnNewPlayers();
	}

	void TrySpawnNewPlayers()
	{
		foreach (InputMethod inputMethod in System.Enum.GetValues(typeof(InputMethod)))
		{
			if (inputMethodToPlayerID.ContainsKey(inputMethod))
			{
				// this controller already controls a player
				continue;
			}

			if (IsButtonDown(inputMethod, ButtonType.Shoot))
			{
				foreach (PlayerID playerID in System.Enum.GetValues(typeof(PlayerID)))
				{
					if (activePlayersById.ContainsKey(playerID))
					{
						// Player already exists
						continue;
					}

					inputMethodToPlayerID[inputMethod] = playerID;
					SpawnNewPlayer(playerID, inputMethod);

					// enought spaning for this input method
					break;
				}

				// all players already occupied.
			}
		}	
	}

	void SpawnNewPlayer(PlayerID playerID, InputMethod inputMethod)
	{
		GameObject newPlayerObject = Instantiate(playerPrefab, gameObject.transform);

		float randRadius = 5.0f;
		newPlayerObject.transform.position = new Vector3(Random.Range(-randRadius, randRadius), 0.0f, Random.Range(-randRadius, randRadius));
		newPlayerObject.GetComponent<InputController>().playerID = playerID;

		Player newPlayer = new Player();
		newPlayer.playerObject = newPlayerObject;
		newPlayer.inputMethod  = inputMethod;

		activePlayersById[playerID] = newPlayer;
	}

    Player GetPlayer(PlayerID playerID)
    {
        if (!activePlayersById.ContainsKey(playerID))
        {
            Debug.Log("Accessing invalid Player " + playerID);
            return new Player();
        }

        return activePlayersById[playerID];
    }

    InputMethod GetInputMethod(PlayerID playerID)
    {
        return GetPlayer(playerID).inputMethod;
    }

    string InputMethodToPostfix(InputMethod inputMethod)
    {
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

    string AxisToPrefix(AxisType axisType)
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

        return "InvalidAxis ";
    }

	string ButtonToPrefix(ButtonType buttonType)
	{
		switch (buttonType)
		{
			case ButtonType.Shoot:
				return "Fire";
		}

		return "InvalidButton ";
	}

	float GetAxisValue(InputMethod inputMethod, AxisType axisType)
	{
		string inputName = AxisToPrefix(axisType) + InputMethodToPostfix(inputMethod);
        
		return Input.GetAxis(inputName);
	}

    public float GetAxisValue(PlayerID playerID, AxisType axisType)
    {
        InputMethod inputMethod = GetInputMethod(playerID);

		return GetAxisValue(inputMethod, axisType);
    }

	bool IsButtonDown(InputMethod inputMethod, ButtonType buttonType)
	{
		string inputName = ButtonToPrefix(buttonType) + InputMethodToPostfix(inputMethod);
        
		return Input.GetButton(inputName);
	}

	public bool IsButtonDown(PlayerID playerID, ButtonType buttonType)
	{
		InputMethod inputMethod = GetInputMethod(playerID);

		return IsButtonDown(inputMethod, buttonType);
	}

	public static PlayerManager instance
    {
        get; private set;
    }
}
