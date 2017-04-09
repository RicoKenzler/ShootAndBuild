using System.Collections;
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

	class InputPlayer
	{
		public InputMethod inputMethod	= InputMethod.Keyboard;
		private float vibrationAmountR	= 0.0f;
		private float vibrationAmountL	= 0.0f;
		private float vibrateUntil		= 0.0f;
		private bool vibrationSleep		= true;

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
	
	private void UpdateVibrations()
	{
		foreach(KeyValuePair<PlayerID, InputPlayer> player in activePlayersById)
		{
			player.Value.UpdateVibration();
		} 
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

            if (WasButtonJustPressed(inputMethod, buttonType))
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

		inputMethodToPlayerID[inputMethod] = playerID;
		activePlayersById[playerID] = newInputPlayer;
	}

	void Awake()
    {
        instance = this;
    }

	// Update is called once per frame
	void Update ()
	{
		UpdateVibrations();
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
		GetInputPlayer(playerID).StartVibration(amountLeft, amountRight, duration);
	}

	public static InputManager instance
    {
        get; private set;
    }
}
