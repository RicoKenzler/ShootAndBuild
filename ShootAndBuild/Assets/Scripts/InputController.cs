using UnityEngine;

public class InputController : MonoBehaviour
{
    public PlayerID playerID;

    public float speed                          = 10.0f;
    public float deadzone                       = 0.2f;
    public float movementAnimationMoultiplier   = 0.5f;

    private Animation animationController;


	void Start()
	{
        animationController = this.GetComponentInChildren<Animation>();
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
        float leftHorizontal    =  PlayerManager.instance.GetAxisValue(playerID, AxisType.LeftAxisH);
        float leftVertical      =  PlayerManager.instance.GetAxisValue(playerID, AxisType.LeftAxisV);
        float rightHorizontal   =  PlayerManager.instance.GetAxisValue(playerID, AxisType.RightAxisH);
        float rightVertical     =  PlayerManager.instance.GetAxisValue(playerID, AxisType.RightAxisV);

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
        // Shoot
        /////////////////////////////////////////
        bool shootButtonPressed = PlayerManager.instance.IsButtonDown(playerID, ButtonType.Shoot);
        Shootable shootable = GetComponent<Shootable>();
        if (shootable != null && shootButtonPressed)
        {
            shootable.Shoot();
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

            if (shootButtonPressed)
            {
                animationController["attack"].speed = 2;
                animationController.Play("attack");
            }

        }
    }
}
