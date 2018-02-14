using UnityEngine;

namespace SAB
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private float m_Speed = 10.0f;
        [SerializeField] private float m_Deadzone = 0.2f;
        [SerializeField] private float m_MovementAnimationMoultiplier = 0.5f;
		[SerializeField] private SpriteRenderer m_PlayerDecal = null;
		[SerializeField] private Light			m_PlayerDecalLight = null;

		///////////////////////////////////////////////////////////////////////////

        private PlayerID		m_PlayerID;
		private Animation		m_AnimationController;
        private TauntController m_TauntController;
        private Shooter		m_Shootable;
        private Mover			m_Movable;
        private Builder			m_Builder;
        private Inventory		m_Inventory;
        private PlayerMenu		m_PlayerMenu;
		private Color			m_PlayerColor;

		///////////////////////////////////////////////////////////////////////////

		public PlayerID playerID	{ get { return m_PlayerID; }	set { m_PlayerID	= value; }}
		public Color	playerColor	{ get { return m_PlayerColor; } set { m_PlayerColor = value; OnChangeColor(); }}

		///////////////////////////////////////////////////////////////////////////

        void Start()
        {
            m_TauntController	= GetComponent<TauntController>();
            m_Shootable			= GetComponent<Shooter>();
            m_Movable			= GetComponent<Mover>();
            m_Builder			= GetComponent<Builder>();
            m_Inventory			= GetComponent<Inventory>();
            m_PlayerMenu		= GetComponent<PlayerMenu>();

            m_AnimationController = GetComponentInChildren<Animation>();
            if (m_AnimationController == null)
            {
                Debug.LogWarning("no animation found on player " + m_PlayerID);
            }
            else
            {
                m_AnimationController["idle"].speed = 1;
                m_AnimationController.Play();
            }

			playerColor = PlayerManager.instance.playerColors[(int) m_PlayerID];
        }

        ///////////////////////////////////////////////////////////////////////////

        void Update()
        {
            if (Time.timeScale == 0.0f)
            {
                return;
            }

            /////////////////////////////////////////
            // Movement
            /////////////////////////////////////////
            float leftHorizontal = InputManager.instance.GetAxisValue(m_PlayerID, AxisType.LeftAxisH);
            float leftVertical = InputManager.instance.GetAxisValue(m_PlayerID, AxisType.LeftAxisV);
            float rightHorizontal = InputManager.instance.GetAxisValue(m_PlayerID, AxisType.RightAxisH);
            float rightVertical = InputManager.instance.GetAxisValue(m_PlayerID, AxisType.RightAxisV);

            Vector3 leftInputVector = new Vector3(leftHorizontal, 0, leftVertical);
            float leftInputVectorLength = leftInputVector.magnitude;

            if (leftInputVectorLength > 1.0f)
            {
                leftInputVector.Normalize();
            }
            else if (Mathf.Abs(leftHorizontal) < m_Deadzone && Mathf.Abs(leftVertical) < m_Deadzone)
            {
                leftInputVector = Vector3.zero;
            }

            m_Movable.moveForce = leftInputVector * m_Speed;

            /////////////////////////////////////////
            // Rotation
            /////////////////////////////////////////
            Vector3 rightInputVector = new Vector3(rightHorizontal, 0, rightVertical);
            float rightInputVectorLength = rightInputVector.magnitude;

            if (Mathf.Abs(rightHorizontal) < m_Deadzone && Mathf.Abs(rightVertical) < m_Deadzone)
            {
                rightInputVector = Vector3.zero;
                rightInputVectorLength = 0;
            }

            if (rightInputVectorLength != 0)
            {
                transform.LookAt(transform.position + rightInputVector);
            }

            /////////////////////////////////////////
            // Buttons
            /////////////////////////////////////////
            bool useItemButtonPressed = InputManager.instance.WasButtonJustPressed(m_PlayerID, ButtonType.UseItem);
            bool shootTriggerPressed = InputManager.instance.IsButtonDown(m_PlayerID, ButtonType.Shoot);

            if (m_Shootable != null && shootTriggerPressed)
            {
                m_Shootable.Shoot();
            }

            if (useItemButtonPressed)
            {
                m_Inventory.TryUseActiveItem();
            }

            if (InputManager.instance.WasButtonJustPressed(m_PlayerID, ButtonType.Taunt))
            {
                m_TauntController.PlayTaunt();
            }

			if (InputManager.instance.WasButtonJustPressed(m_PlayerID, ButtonType.Build))
			{
				m_Builder.ShowBuildPreview();
			}
			else if (InputManager.instance.IsButtonReleased(m_PlayerID, ButtonType.Build))
			{
				m_Builder.HideBuildPreview();
                m_Builder.TryBuild();
			}

            /////////////////////////////////////////
            /// Menu Navigation
            /////////////////////////////////////////

            bool positive;
            if (InputManager.instance.WasAxisJustPressed(m_PlayerID, AxisType.MenuH, out positive))
            {
                m_PlayerMenu.ChangeSelectionCategory(positive);
            }

            if (InputManager.instance.WasAxisJustPressed(m_PlayerID, AxisType.MenuV, out positive))
            {
                m_PlayerMenu.CycleThroughCategory(positive);
            }

            /////////////////////////////////////////
            // Animation
            /////////////////////////////////////////

            if (m_AnimationController != null)
            {
                float movementSpeed = m_Movable.moveForce.magnitude;

                if (!m_AnimationController.IsPlaying("attack"))
                {
                    if (movementSpeed > 0)
                    {
                        m_AnimationController["walk"].speed = movementSpeed * m_MovementAnimationMoultiplier;
                        m_AnimationController.Play("walk");
                    }
                    else
                    {
                        m_AnimationController.Play("idle");
                    }
                }

                if (useItemButtonPressed)
                {
                    m_AnimationController["attack"].speed = 2;
                    m_AnimationController.Play("attack");
                }
            }

        }

		void OnChangeColor()
		{
			m_PlayerDecal.color = m_PlayerColor;
			m_PlayerDecalLight.color = m_PlayerColor;
		}
    }
}