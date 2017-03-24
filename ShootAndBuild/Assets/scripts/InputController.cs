using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public int PlayerID = 1;

    public float Speed              = 10.0f;
    public float Deadzone           = 0.2f;

    private Animation m_Animation;
    public float m_MovementAnimationMoultiplier = 0.5f;

	// Use this for initialization
	void Start ()
	{
        m_Animation = this.GetComponentInChildren<Animation>();
        if (m_Animation == null)
        {
            Debug.LogWarning("no animation found on player " + PlayerID);
        } else
        {
            m_Animation["girl_idle"].speed = 1;
            m_Animation.Play();
        }
	}
	
    //--------------------------------------------------------------------------------------------------

	// Update is called once per frame
	void Update ()
    {
        /////////////////////////////////////////
        // Movement
        /////////////////////////////////////////
        float leftHorizontal    =  Input.GetAxis("Left Horizontal P"    + PlayerID);
        float leftVertical      = -Input.GetAxis("Left Vertical P"      + PlayerID);
        float rightHorizontal   =  Input.GetAxis("Right Horizontal P"   + PlayerID);
        float rightVertical     = -Input.GetAxis("Right Vertical P"     + PlayerID);

        // For player 2, also accept Keyboard (for debugging)
        if (PlayerID == 2)
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
        else if (Mathf.Abs(leftHorizontal) < Deadzone && Mathf.Abs(leftVertical) < Deadzone)
        {
            leftInputVector = Vector2.zero;
        }

		Rigidbody rigidbody = GetComponent<Rigidbody>();
        Vector3 velocity    = rigidbody.velocity;
        velocity.x		    = leftInputVector.x * Speed;
        velocity.z		    = leftInputVector.y * Speed;

        rigidbody.velocity = velocity;

        /////////////////////////////////////////
        // Rotation
        /////////////////////////////////////////
        Vector3 rightInputVector = new Vector3(rightHorizontal, 0, rightVertical);
        float rightInputVectorLength = rightInputVector.magnitude;

        if (Mathf.Abs(rightHorizontal) < Deadzone && Mathf.Abs(rightVertical) < Deadzone)
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
        bool shootButtonPressed = Input.GetButton("Fire P" + PlayerID);
        Shootable shootable = GetComponent<Shootable>();
        if (shootable != null && shootButtonPressed)
        {
            shootable.Shoot();
        }


        /////////////////////////////////////////
        // Animation
        /////////////////////////////////////////

        if (m_Animation != null)
        {
            float movementSpeed = velocity.magnitude;

            if (!m_Animation.IsPlaying("girl_attack"))
            {
                if (movementSpeed > 0)
                {
                    m_Animation["girl_run"].speed = movementSpeed * m_MovementAnimationMoultiplier;
                    m_Animation.Play("girl_run");
                }
                else
                {
                    m_Animation.Play("girl_idle");
                }
            }

            if (shootButtonPressed)
            {
                m_Animation["girl_attack"].speed = 2;
                m_Animation.Play("girl_attack");
            }

        }
    }
}
