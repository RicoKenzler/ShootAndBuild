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
/*
    Player[] activePlayersById;


	// Use this for initialization
	void Start ()
    {
		
	}

	// Update is called once per frame
	void Update ()
    {
		TrySpawnNewPlayers();
	}

	void TrySpawnNewPlayers()
	{
		foreach (InputMethod inputMethod in System.Enum.GetValues(typeof(InputMethod))
		{
			if not alrady assigned;

			if (IsButtonDown(method, ButtonType.Shoot))
			{
				SpawnNewPlayer(inputMethod);
			}
		}	
	}

	void SpawnNewPlayer(InputMethod inputMethod)
	{
		TODO;
	}

    Player GetPlayer(uint playerID)
    {
        if (activePlayersById.Length <= playerID)
        {
            Debug.Log("Accessing invalid Player " + playerID);
            return new Player();
        }

        return activePlayersById[playerID];
    }

    InputMethod GetInputMethod(uint playerID)
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

    public float GetAxisValue(uint playerID, AxisType axisType)
    {
        InputMethod inputMethod = GetInputMethod(playerID);

		return GetAxisValue(inputMethod, axisType);
    }

	bool IsButtonDown(InputMethod inputMethod, ButtonType buttonType)
	{
		string inputName = ButtonToPrefix(buttonType) + InputMethodToPostfix(inputMethod);
        
		return Input.GetButton(inputName);
	}

	public bool IsButtonDown(uint playerID, ButtonType buttonType)
	{
		InputMethod inputMethod = GetInputMethod(playerID);

		return IsButtonDown(inputMethod, buttonType);
	}
	*/
}
