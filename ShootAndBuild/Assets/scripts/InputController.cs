using UnityEngine;

public class InputController : MonoBehaviour
{
    public int playerID = 1;

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
        float leftHorizontal    =  Input.GetAxis("Left Horizontal P"    + playerID);
        float leftVertical      =  Input.GetAxis("Left Vertical P"      + playerID);
        float rightHorizontal   =  Input.GetAxis("Right Horizontal P"   + playerID);
        float rightVertical     =  Input.GetAxis("Right Vertical P"     + playerID);

        // For player 2, also accept Keyboard (for debugging)
        if (playerID == 2)
        {
            float leftHorizontalKey     = Input.GetAxis("Left Horizontal Keyboard");
            float leftVerticalKey       = Input.GetAxis("Left Vertical Keyboard");
            float rightHorizontalKey    = Input.GetAxis("Right Horizontal Keyboard");
            float rightVerticalKey      = Input.GetAxis("Right Vertical Keyboard");

            leftHorizontal  = (Mathf.Abs(leftHorizontalKey)     > Mathf.Abs(leftHorizontal))    ? leftHorizontalKey     : leftHorizontal;
            leftVertical    = (Mathf.Abs(leftVerticalKey)       > Mathf.Abs(leftVertical))      ? leftVerticalKey       : leftVertical;
            rightHorizontal = (Mathf.Abs(rightHorizontalKey)    > Mathf.Abs(rightHorizontal))   ? rightHorizontalKey    : rightHorizontal;
            rightVertical   = (Mathf.Abs(rightVerticalKey)      > Mathf.Abs(rightVertical))     ? rightVerticalKey      : rightVertical;
        }

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
        bool shootButtonPressed = Input.GetButton("Fire P" + playerID) || (playerID == 2 && Input.GetButton("Fire Keyboard"));
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
