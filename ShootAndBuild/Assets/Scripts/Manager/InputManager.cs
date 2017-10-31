using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using XInputDotNetPure;

namespace SAB
{
    public enum AxisType
    {
        LeftAxisH,
        LeftAxisV,

        RightAxisH,
        RightAxisV,

        MenuH,
        MenuV,
    }

	///////////////////////////////////////////////////////////////////////////

    public enum ButtonType
    {
        Taunt,
        Build,
        UseItem,
        Shoot,
        Start,
		Restart,

        Unused,
    }
	
	///////////////////////////////////////////////////////////////////////////

    public enum InputMethod
    {
        Keyboard,

        Gamepad1,
        Gamepad2,
        Gamepad3,
        Gamepad4,
    }

	///////////////////////////////////////////////////////////////////////////

    public class InputManager : MonoBehaviour
    {
        [SerializeField] InputMethod m_DebugKeyboardEmulates = InputMethod.Keyboard;

		///////////////////////////////////////////////////////////////////////////

        private Dictionary<PlayerID, InputPlayer> m_ActivePlayersById = new Dictionary<PlayerID, InputPlayer>();
        private Dictionary<InputMethod, PlayerID> m_InputMethodToPlayerID = new Dictionary<InputMethod, PlayerID>();

        private const float TRIGGER_DOWN_THRESHOLD = 0.3f;

		///////////////////////////////////////////////////////////////////////////

		public static InputManager instance	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

        bool UsesDebugEmulation()
        {
            return (m_DebugKeyboardEmulates != InputMethod.Keyboard);
        }

		///////////////////////////////////////////////////////////////////////////

        string ApplyDebugEmulationOnString(string inputIdentifier)
        {
            Debug.Assert(m_DebugKeyboardEmulates != InputMethod.Keyboard);

            // For debugging: switch keyboard with emulated inputMethod
            string strEmulated = InputMethodToPostfix(m_DebugKeyboardEmulates);
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

		///////////////////////////////////////////////////////////////////////////

        class InputPlayer
        {
            // Why do we track button states on our own?
            // 1) we want to treat triggers (=axes) and buttons (=buttons) the same way, but UnityInputManager has no concept of
            //    WasTriggerJustPressed
            // 2) so we do not need to concatenate strings on the fly
            class PlayerButtonInfos
            {
                public string buttonIdentifier;
                public bool wasJustPressedState;
                public bool isDownState;

                public PlayerButtonInfos(string identifier)
                {
                    buttonIdentifier = identifier;
                    wasJustPressedState = false;
                    isDownState = false;
                }
            }

			///////////////////////////////////////////////////////////////////////////

            class PlayerAxisInfos
            {
                public string axisIdentifier;
                public bool wasJustPressedState;
                public float axisValue;
                public int isDownState;     // -1) negative, 0) none, 1) positive

                public PlayerAxisInfos(string identifier)
                {
                    axisIdentifier = identifier;
                    wasJustPressedState = false;
                    axisValue = 0.0f;
                    isDownState = 0;
                }
            }

			///////////////////////////////////////////////////////////////////////////

            private Dictionary<ButtonType, PlayerButtonInfos>	m_ButtonInfos	= new Dictionary<ButtonType, PlayerButtonInfos>();
            private Dictionary<AxisType, PlayerAxisInfos>		m_AxisInfos		= new Dictionary<AxisType, PlayerAxisInfos>();

            private InputMethod m_InputMethod		= InputMethod.Keyboard;
            private float		m_VibrationAmountR	= 0.0f;
            private float		m_VibrationAmountL	= 0.0f;
            private float		m_VibrateUntil		= 0.0f;
            private bool		m_VibrationSleep	= true;

			///////////////////////////////////////////////////////////////////////////

			public InputMethod inputMethod		{ get { return m_InputMethod; } set { m_InputMethod = value; } }

			///////////////////////////////////////////////////////////////////////////

            public void InitButtonStates()
            {
                m_ButtonInfos.Clear();
                m_AxisInfos.Clear();

                foreach (ButtonType buttonType in System.Enum.GetValues(typeof(ButtonType)))
                {
                    string inputIdentifier = InputManager.instance.ButtonToPrefix(buttonType) + InputManager.instance.InputMethodToPostfix(m_InputMethod);
                    m_ButtonInfos[buttonType] = new PlayerButtonInfos(inputIdentifier);
                }

                foreach (AxisType axisType in System.Enum.GetValues(typeof(AxisType)))
                {
                    string inputIdentifier = InputManager.instance.AxisToPrefix(axisType) + InputManager.instance.InputMethodToPostfix(m_InputMethod);
                    m_AxisInfos[axisType] = new PlayerAxisInfos(inputIdentifier);
                }

                UpdateButtonStates();
            }

			///////////////////////////////////////////////////////////////////////////

            PlayerButtonInfos GetButtonInfos(ButtonType buttonType)
            {
                PlayerButtonInfos outState = null;
                if (!m_ButtonInfos.TryGetValue(buttonType, out outState))
                {
                    Debug.Log("Trying to access unknown button " + buttonType);
                }

                return outState;
            }

			///////////////////////////////////////////////////////////////////////////

            PlayerAxisInfos GetAxisInfos(AxisType axisType)
            {
                PlayerAxisInfos outState = null;
                if (!m_AxisInfos.TryGetValue(axisType, out outState))
                {
                    Debug.Log("Trying to access unknown axis " + axisType);
                }

                return outState;
            }

			///////////////////////////////////////////////////////////////////////////

            public float GetAxisValue(AxisType axisType)
            {
                return GetAxisInfos(axisType).axisValue;
            }
			
			///////////////////////////////////////////////////////////////////////////

            public bool IsButtonDown(ButtonType buttonType)
            {
                PlayerButtonInfos buttonInfos = GetButtonInfos(buttonType);
                return buttonInfos.isDownState;
            }
			
			///////////////////////////////////////////////////////////////////////////

            public bool WasButtonJustPressed(ButtonType buttonType)
            {
                PlayerButtonInfos buttonInfos = GetButtonInfos(buttonType);
                return buttonInfos.wasJustPressedState;
            }
			
			///////////////////////////////////////////////////////////////////////////

            public bool WasAxisJustPressed(AxisType axisType, out bool positive)
            {
                PlayerAxisInfos axisInfos = GetAxisInfos(axisType);
                positive = axisInfos.axisValue > 0.0f;

                return axisInfos.wasJustPressedState;
            }
			
			///////////////////////////////////////////////////////////////////////////

            public void UpdateButtonStates()
            {
                foreach (KeyValuePair<ButtonType, PlayerButtonInfos> buttonInfoPair in m_ButtonInfos)
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

                    bool isButtonDown = (axisValue > TRIGGER_DOWN_THRESHOLD);

                    infos.wasJustPressedState = (!infos.isDownState && isButtonDown);
                    infos.isDownState = isButtonDown;
                }

                foreach (KeyValuePair<AxisType, PlayerAxisInfos> axisInfoPair in m_AxisInfos)
                {
                    PlayerAxisInfos infos = axisInfoPair.Value;

                    float axisValue;

                    if (InputManager.instance.UsesDebugEmulation())
                    {
                        axisValue = Input.GetAxisRaw(InputManager.instance.ApplyDebugEmulationOnString(infos.axisIdentifier));
                    }
                    else
                    {
                        axisValue = Input.GetAxisRaw(infos.axisIdentifier);
                    }

                    int isDownState = 0;

                    if (axisValue > TRIGGER_DOWN_THRESHOLD)
                    {
                        isDownState = 1;
                    }
                    else if (axisValue < -TRIGGER_DOWN_THRESHOLD)
                    {
                        isDownState = -1;
                    }

                    infos.wasJustPressedState = (isDownState != 0 && (isDownState != infos.isDownState));
                    infos.axisValue = axisValue;
                    infos.isDownState = isDownState;
                }
            }
			
			///////////////////////////////////////////////////////////////////////////

            public void StartVibration(float amountL, float amountR, float duration)
            {
                if (amountL <= m_VibrationAmountL && amountR <= m_VibrationAmountR)
                {
                    // dont overwrite hard vibration with soft vibration
                    return;
                }

                m_VibrationAmountL = amountL;
                m_VibrationAmountR = amountR;
                m_VibrateUntil = Time.unscaledTime + duration;
                m_VibrationSleep = false;
            }
			
			///////////////////////////////////////////////////////////////////////////

            private int GetXInputPlayerIndex()
            {
                switch (m_InputMethod)
                {
                    case InputMethod.Gamepad1: return 0;
                    case InputMethod.Gamepad2: return 1;
                    case InputMethod.Gamepad3: return 2;
                    case InputMethod.Gamepad4: return 3;
                }

                return -1;
            }
			
			///////////////////////////////////////////////////////////////////////////

            public void UpdateVibration()
            {
                if (m_VibrationSleep)
                {
                    return;
                }

                if (CheatManager.instance.disableVibration || (Time.unscaledTime > m_VibrateUntil))
                {
                    // We vibrated long enough
                    m_VibrationAmountL = 0.0f;
                    m_VibrationAmountR = 0.0f;
                    m_VibrationSleep = true;
                }

                int xInputIndex = GetXInputPlayerIndex();

                if (xInputIndex == -1)
                {
                    // Player has no vibrating device
                    return;
                }

                GamePad.SetVibration((PlayerIndex)xInputIndex, m_VibrationAmountL, m_VibrationAmountR);
            }
        }

		///////////////////////////////////////////////////////////////////////////

        public InputMethod? IsButtonDownForUnusedInputMethod(ButtonType buttonType)
        {
            foreach (InputMethod inputMethod in System.Enum.GetValues(typeof(InputMethod)))
            {
                if (m_InputMethodToPlayerID.ContainsKey(inputMethod))
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
		
		///////////////////////////////////////////////////////////////////////////

        public void OnSpawnNewPlayer(PlayerID playerID, InputMethod inputMethod)
        {
            InputPlayer newInputPlayer = new InputPlayer();
            newInputPlayer.inputMethod = inputMethod;
            newInputPlayer.InitButtonStates();

            m_InputMethodToPlayerID[inputMethod] = playerID;
            m_ActivePlayersById[playerID] = newInputPlayer;
        }
		
		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;
        }
		
		///////////////////////////////////////////////////////////////////////////

        // Update is called once per frame
        void Update()
        {
            foreach (KeyValuePair<PlayerID, InputPlayer> player in m_ActivePlayersById)
            {
                player.Value.UpdateVibration();
                player.Value.UpdateButtonStates();
            }
        }
		
		///////////////////////////////////////////////////////////////////////////

        private InputPlayer GetInputPlayer(PlayerID playerID)
        {
            if (!m_ActivePlayersById.ContainsKey(playerID))
            {
                Debug.Log("Accessing invalid Player " + playerID);
                return new InputPlayer();
            }

            return m_ActivePlayersById[playerID];
        }
		
		///////////////////////////////////////////////////////////////////////////

        private InputMethod GetInputMethod(PlayerID playerID)
        {
            return GetInputPlayer(playerID).inputMethod;
        }
		
		///////////////////////////////////////////////////////////////////////////

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
		
		///////////////////////////////////////////////////////////////////////////

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

                case AxisType.MenuH:
                    return "Menu Horizontal";
                case AxisType.MenuV:
                    return "Menu Vertical";
            }

            return "InvalidAxis";
        }
		
		///////////////////////////////////////////////////////////////////////////

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
                case ButtonType.Start:
                    return "Start";
				case ButtonType.Restart:
                    return "Restart";
            }

            return "InvalidButton";
        }
		
		///////////////////////////////////////////////////////////////////////////

        public float GetAxisValue(PlayerID playerID, AxisType axisType)
        {
            InputPlayer player = GetInputPlayer(playerID);

            return player.GetAxisValue(axisType);
        }
		
		///////////////////////////////////////////////////////////////////////////

        public bool IsButtonDown(PlayerID playerID, ButtonType buttonType)
        {
            InputPlayer player = GetInputPlayer(playerID);

            return player.IsButtonDown(buttonType);
        }
		
		///////////////////////////////////////////////////////////////////////////

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
		
		///////////////////////////////////////////////////////////////////////////

        public bool WasButtonJustPressed(PlayerID playerID, ButtonType buttonType)
        {
            InputPlayer player = GetInputPlayer(playerID);

            return player.WasButtonJustPressed(buttonType);
        }
		
		///////////////////////////////////////////////////////////////////////////

        public void SetVibration(PlayerID playerID, float amountLeft, float amountRight, float duration)
        {
            GetInputPlayer(playerID).StartVibration(amountLeft, amountRight, duration);
        }
		
		///////////////////////////////////////////////////////////////////////////

        public void SetVibrationAll(float amountLeft, float amountRight, float duration)
        {
            foreach (InputController player in PlayerManager.instance.allAlivePlayers)
            {
                SetVibration(player.playerID, amountLeft, amountRight, duration);
            }
        }
		
		///////////////////////////////////////////////////////////////////////////

        public bool WasAxisJustPressed(PlayerID playerID, AxisType axisType, out bool positive)
        {
            InputPlayer inputPlayer = GetInputPlayer(playerID);

            return inputPlayer.WasAxisJustPressed(axisType, out positive);
        }
		
		///////////////////////////////////////////////////////////////////////////

        public bool DidAnyPlayerJustPress(ButtonType buttonType)
        {
            foreach (KeyValuePair<PlayerID, InputPlayer> activePlayer in m_ActivePlayersById)
            {
                if (activePlayer.Value.WasButtonJustPressed(buttonType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}