using UnityEngine;

public class InputController : MonoBehaviour
{
    public PlayerID playerID;

    public float		speed                          = 10.0f;
    public float		deadzone                       = 0.2f;
    public float		movementAnimationMoultiplier   = 0.5f;

    private Animation		animationController;
	private TauntController tauntController;
	private Shootable		shootable;
	private Movable			movable;
	private Builder			builder;
	private Inventory		inventory;
	private PlayerMenu		playerMenu;

	void Start()
	{
		tauntController	= GetComponent<TauntController>();
		shootable		= GetComponent<Shootable>();
		movable			= GetComponent<Movable>();
		builder			= GetComponent<Builder>();
		inventory		= GetComponent<Inventory>();
		playerMenu		= GetComponent<PlayerMenu>();

		animationController = GetComponentInChildren<Animation>();
        if (animationController == null)
        {
            Debug.LogWarning("no animation found on player " + playerID);
        } else
        {
            animationController["idle"].speed = 1;
            animationController.Play();
        }
	}

	//--------------------------------------------------------------------------------------------------

	void Update()
    {
        /////////////////////////////////////////
        // Movement
        /////////////////////////////////////////
        float leftHorizontal    =  InputManager.instance.GetAxisValue(playerID, AxisType.LeftAxisH);
        float leftVertical      =  InputManager.instance.GetAxisValue(playerID, AxisType.LeftAxisV);
        float rightHorizontal   =  InputManager.instance.GetAxisValue(playerID, AxisType.RightAxisH);
        float rightVertical     =  InputManager.instance.GetAxisValue(playerID, AxisType.RightAxisV);

        Vector3 leftInputVector = new Vector3(leftHorizontal, 0, leftVertical);
        float leftInputVectorLength = leftInputVector.magnitude;
        
        if (leftInputVectorLength > 1.0f)
        {
            leftInputVector.Normalize();
        }
        else if (Mathf.Abs(leftHorizontal) < deadzone && Mathf.Abs(leftVertical) < deadzone)
        {
            leftInputVector = Vector3.zero;
        }

		movable.moveForce = leftInputVector * speed;

        /////////////////////////////////////////
        // Rotation
        /////////////////////////////////////////
        Vector3 rightInputVector = new Vector3(rightHorizontal, 0, rightVertical);
        float rightInputVectorLength = rightInputVector.magnitude;

        if (Mathf.Abs(rightHorizontal) < deadzone && Mathf.Abs(rightVertical) < deadzone)
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
        bool useItemButtonPressed	= InputManager.instance.WasButtonJustPressed(playerID, ButtonType.UseItem);
		bool shootTriggerPressed	= InputManager.instance.IsButtonDown(playerID, ButtonType.Shoot);

        if (shootable != null && shootTriggerPressed)
        {
            shootable.Shoot();
        }

		if (useItemButtonPressed)
		{
			inventory.TryUseActiveItem();
		}

		if (InputManager.instance.WasButtonJustPressed(playerID, ButtonType.Taunt))
        {
			tauntController.PlayTaunt();
		}

		if (InputManager.instance.WasButtonJustPressed(playerID, ButtonType.Build))
		{
			builder.TryBuild();
		}

		/////////////////////////////////////////
		/// Menu Navigation
		/////////////////////////////////////////

		bool positive;
		if (InputManager.instance.WasAxisJustPressed(playerID, AxisType.MenuH, out positive))
		{
			playerMenu.ChangeSelectionCategory(positive);
		}
		
		if (InputManager.instance.WasAxisJustPressed(playerID, AxisType.MenuV, out positive))
		{
			playerMenu.ChangeActiveWithinCategory(positive);
		}

		/////////////////////////////////////////
		// Animation
		/////////////////////////////////////////

		if (animationController != null)
        {
			float movementSpeed = movable.moveForce.magnitude;

            if (!animationController.IsPlaying("attack"))
            {
                if (movementSpeed > 0)
                {
                    animationController["walk"].speed = movementSpeed * movementAnimationMoultiplier;
                    animationController.Play("walk");
                }
                else
                {
                    animationController.Play("idle");
                }
            }

            if (useItemButtonPressed)
            {
                animationController["attack"].speed = 2;
                animationController.Play("attack");
            }
        }

    }
}
