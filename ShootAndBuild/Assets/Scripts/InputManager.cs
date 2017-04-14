using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using XInputDotNetPure;

public enum AxisType
{
    LeftAxisH,
    LeftAxisV,

    RightAxisH,
    RightAxisV,
}

public enum ButtonType
{
    Unused,
	Taunt,
	Build,

	UseItem,
	Shoot,
}

public enum InputMethod
{
	Keyboard,

	Gamepad1,
	Gamepad2,
	Gamepad3,
	Gamepad4,
}

public class InputManager : MonoBehaviour
{
	[SerializeField]
	InputMethod debugKeyboardEmulates;

	private Dictionary<PlayerID, InputPlayer> activePlayersById		= new Dictionary<PlayerID, InputPlayer>();
    private Dictionary<InputMethod, PlayerID> inputMethodToPlayerID = new Dictionary<InputMethod, PlayerID>();

	private const float TRIGGER_DOWN_THRESHOLD = 0.3f;

	bool UsesDebugEmulation()
	{
		return (debugKeyboardEmulates != InputMethod.Keyboard);
	}

	string ApplyDebugEmulationOnString(string inputIdentifier)
	{
		Debug.Assert(debugKeyboardEmulates != InputMethod.Keyboard);

		// For debugging: switch keyboard with emulated inputMethod
		string strEmulated = InputMethodToPostfix(debugKeyboardEmulates);
		string strKeyboard = InputMethodToPostfix(InputMethod.Keyboard);

		if (inputIdentifier.Contains(strEmulated))
		{
			return inputIdentifier.Replace(strEmulated, strKeyboard);
		}
		else
		{
			return inputIdentifier.Replace(strKeyboard, strEmulated);
		}
	}
	
	class InputPlayer
	{
		// Why do we track button states on our own?
		// 1) we want to treat triggers (=axes) and buttons (=buttons) the same way, but UnityInputManager has no concept of
		//    WasTriggerJustPressed
		// 2) so we do not need to concatenate strings on the fly
		class PlayerButtonInfos
		{
			public string	buttonIdentifier;
			public bool		wasJustPressedState;
			public bool		isDownState;

			public PlayerButtonInfos(string identifier)
			{
				buttonIdentifier		= identifier;
				wasJustPressedState		= false;
				isDownState				= false;
			}
		}

		private Dictionary<ButtonType, PlayerButtonInfos> buttonInfos = new Dictionary<ButtonType, PlayerButtonInfos>();

		public InputMethod inputMethod	= InputMethod.Keyboard;
		private float vibrationAmountR	= 0.0f;
		private float vibrationAmountL	= 0.0f;
		private float vibrateUntil		= 0.0f;
		private bool vibrationSleep		= true;

		public void InitButtonStates()
		{
			buttonInfos.Clear();

			foreach (ButtonType buttonType in System.Enum.GetValues(typeof(ButtonType)))
			{
				string inputIdentifier = InputManager.instance.ButtonToPrefix(buttonType) + InputManager.instance.InputMethodToPostfix(inputMethod);
				buttonInfos[buttonType] = new PlayerButtonInfos(inputIdentifier);
			}

			UpdateButtonStates();
		}

		PlayerButtonInfos GetButtonInfos(ButtonType buttonType)
		{
			PlayerButtonInfos outState = null;
			if (!buttonInfos.TryGetValue(buttonType, out outState))
			{
				Debug.Log("Trying to access unknown button " + buttonType);
			}

			return outState;
		}

		public bool IsButtonDown(ButtonType buttonType)
		{
			PlayerButtonInfos buttonInfos = GetButtonInfos(buttonType);
			return buttonInfos.isDownState;
		}

		public bool WasButtonJustPressed(ButtonType buttonType)
		{
			PlayerButtonInfos buttonInfos = GetButtonInfos(buttonType);
			return buttonInfos.wasJustPressedState;
		}

		public void UpdateButtonStates()
		{
			foreach (KeyValuePair<ButtonType, PlayerButtonInfos> buttonInfoPair in buttonInfos)
			{
				PlayerButtonInfos infos = buttonInfoPair.Value;

				float axisValue;

				if (InputManager.instance.UsesDebugEmulation())
				{
					axisValue = Input.GetAxisRaw(InputManager.instance.ApplyDebugEmulationOnString(infos.buttonIdentifier));
				}
				else
				{
					axisValue = Input.GetAxisRaw(infos.buttonIdentifier); 
				}

				bool isButtonDown	= (axisValue > TRIGGER_DOWN_THRESHOLD);

				infos.wasJustPressedState	= (!infos.isDownState && isButtonDown);
				infos.isDownState			= isButtonDown;
			}
		}

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
			vibrationSleep		= false;
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
			if (vibrationSleep)
			{
				return;
			} 

			if (Time.unscaledTime > vibrateUntil)
			{
				// We vibrated long enough
				vibrationAmountL	= 0.0f;
				vibrationAmountR	= 0.0f;
				vibrationSleep = true;
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

	// Use this for initialization
	void Start ()
	{
		
	}

	public InputMethod? IsButtonDownForUnusedInputMethod(ButtonType buttonType)
	{
		foreach (InputMethod inputMethod in System.Enum.GetValues(typeof(InputMethod)))
        {
            if (inputMethodToPlayerID.ContainsKey(inputMethod))
            {
                // this controller already controls a player
                continue;
            }

            if (IsButtonDown(inputMethod, buttonType))
            {
                return inputMethod;
            }
        }

		return null;
	}

	public void OnSpawnNewPlayer(PlayerID playerID, InputMethod inputMethod)
	{
		InputPlayer newInputPlayer = new InputPlayer();
		newInputPlayer.inputMethod = inputMethod;
		newInputPlayer.InitButtonStates();

		inputMethodToPlayerID[inputMethod] = playerID;
		activePlayersById[playerID] = newInputPlayer;
	}

	void Awake()
    {
        instance = this;
    }

	// Update is called once per frame
	void Update()
	{
		foreach(KeyValuePair<PlayerID, InputPlayer> player in activePlayersById)
		{
			player.Value.UpdateVibration();
			player.Value.UpdateButtonStates();
		} 
	}

	private InputPlayer GetInputPlayer(PlayerID playerID)
    {
        if (!activePlayersById.ContainsKey(playerID))
        {
            Debug.Log("Accessing invalid Player " + playerID);
            return new InputPlayer();
        }

        return activePlayersById[playerID];
    }

	private InputMethod GetInputMethod(PlayerID playerID)
    {
        return GetInputPlayer(playerID).inputMethod;
    }

    public string InputMethodToPostfix(InputMethod inputMethod)
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

    public string ButtonToPrefix(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.Unused:
                return "Unused";
			case ButtonType.Taunt:
				return "Taunt";
			case ButtonType.Build:
				return "Build";

			case ButtonType.UseItem:
				return "UseItem";
			case ButtonType.Shoot:
				return "Shoot";
        }

        return "InvalidButton ";
    }

    private float GetAxisValue(InputMethod inputMethod, AxisType axisType)
    {
        string inputName = AxisToPrefix(axisType) + InputMethodToPostfix(inputMethod);

		if (UsesDebugEmulation())
		{
			inputName = ApplyDebugEmulationOnString(inputName);
		}

        return Input.GetAxis(inputName);
    }

    public float GetAxisValue(PlayerID playerID, AxisType axisType)
    {
        InputMethod inputMethod = GetInputMethod(playerID);

        return GetAxisValue(inputMethod, axisType);
	}

    public bool IsButtonDown(PlayerID playerID, ButtonType buttonType)
    {
		InputPlayer player = GetInputPlayer(playerID);

		return player.IsButtonDown(buttonType);
    }

    public bool IsButtonDown(InputMethod inputMethod, ButtonType buttonType)
    {
		string buttonIdentifier = ButtonToPrefix(buttonType) + InputMethodToPostfix(inputMethod);

		if (UsesDebugEmulation())
		{
			buttonIdentifier = ApplyDebugEmulationOnString(buttonIdentifier);
		}

		float axisValue = Input.GetAxisRaw(buttonIdentifier);

		return (axisValue > TRIGGER_DOWN_THRESHOLD);
    }

	public bool WasButtonJustPressed(PlayerID playerID, ButtonType buttonType)
	{
		InputPlayer player = GetInputPlayer(playerID);

		return player.WasButtonJustPressed(buttonType);
	}

	public void SetVibration(PlayerID playerID, float amountLeft, float amountRight, float duration)
	{
		GetInputPlayer(playerID).StartVibration(amountLeft, amountRight, duration);
	}

	public void SetVibrationAll(float amountLeft, float amountRight, float duration)
	{
		foreach(InputController player in PlayerManager.instance.allAlivePlayers)
		{
			SetVibration(player.playerID, amountLeft, amountRight, duration);
		} 
	}

	public static InputManager instance
    {
        get; private set;
    }
}
