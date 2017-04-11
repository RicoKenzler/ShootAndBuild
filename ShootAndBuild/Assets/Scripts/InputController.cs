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
	private Builder			builder;
	private Throwable		throwable;

	void Start()
	{
        animationController = GetComponentInChildren<Animation>();
        if (animationController == null)
        {
            Debug.LogWarning("no animation found on player " + playerID);
        } else
        {
            animationController["idle"].speed = 1;
            animationController.Play();
        }

		tauntController	= GetComponent<TauntController>();
		shootable		= GetComponent<Shootable>();
		builder			= GetComponent<Builder>();
		throwable		= GetComponent<Throwable>();
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

        Vector2 leftInputVector = new Vector2(leftHorizontal, leftVertical);
        float leftInputVectorLength = leftInputVector.magnitude;
        
        if (leftInputVectorLength > 1.0f)
        {
            leftInputVector.Normalize();
        }
        else if (Mathf.Abs(leftHorizontal) < deadzone && Mathf.Abs(leftVertical) < deadzone)
        {
            leftInputVector = Vector2.zero;
        }

		Rigidbody rigidbody = GetComponent<Rigidbody>();
        Vector3 velocity    = rigidbody.velocity;
        velocity.x		    = leftInputVector.x * speed;
        velocity.z		    = leftInputVector.y * speed;

        rigidbody.velocity = velocity;

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
        bool useItemButtonPressed  = InputManager.instance.IsButtonDown(playerID, ButtonType.UseItem);
		bool shootTriggerPressed = InputManager.instance.IsButtonDown(playerID, ButtonType.Shoot);

        if (shootable != null && shootTriggerPressed)
        {
            shootable.Shoot();
        }

		if (throwable != null && useItemButtonPressed)
		{
			throwable.Throw();
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
		// Animation
		/////////////////////////////////////////

		if (animationController != null)
        {
            float movementSpeed = velocity.magnitude;

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
