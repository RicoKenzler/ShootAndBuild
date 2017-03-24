using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public float speed = 10;
    public float attackDistance = 1;
    public float attackCooldown = 1;
	public int damage = 1;

    private float currentAttackCooldown = 0;

	public AudioClip[] HitSounds;

	private Animation m_Animation;

	void Start ()
    {
		m_Animation = this.GetComponentInChildren<Animation>();
        if (m_Animation == null)
        {
            Debug.LogWarning("no animation found on enemy");
        } else
        {
            m_Animation["idle"].speed = 1;
            m_Animation.Play();
        }
	}
	
	void Update ()
    {
        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
            if (currentAttackCooldown < 0)
            {
                currentAttackCooldown = 0;
            }
        }

        // use the input controllers to find players. Obviously we should change this by another criteria. But hey, fuck it! Quick and dirty, man!
        InputController[] players = FindObjectsOfType<InputController>();

        GameObject nearestPlayer = null;
        float nearestDist = float.MaxValue;

        foreach (InputController player in players)
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance < nearestDist)
            {
                nearestDist = distance;
                nearestPlayer = player.gameObject;
            }
        }

        if (!nearestPlayer)
        {
            return;
        }

        Vector3 direction = (nearestPlayer.transform.position - transform.position).normalized;
        transform.LookAt(nearestPlayer.transform);

        if (nearestDist > attackDistance)
        {
            Vector3 velocity = direction * speed;
			GetComponent<Rigidbody>().velocity = velocity;

            //transform.position = transform.position + step;
        }
        else if (currentAttackCooldown == 0)
        {
            currentAttackCooldown = attackCooldown;
            nearestPlayer.GetComponent<Attackable>().DealDamage(damage);

			if (m_Animation)
			{
				m_Animation["attack"].speed = 4.0f;
				m_Animation.Play("attack");
			}

			if (HitSounds.Length > 0)
			{
				int rndSoundIndex = Random.Range(0, HitSounds.Length -1);
				AudioClip rndSound = HitSounds[rndSoundIndex];
				AudioSource.PlayClipAtPoint(rndSound, transform.position, 0.5f);
			}
        }


	    /////////////////////////////////////////
        // Animation
        /////////////////////////////////////////
        if (m_Animation != null)
        {
			float movementSpeed = 1.0f; 

            if (!m_Animation.IsPlaying("attack"))
            {
				m_Animation["move"].speed = movementSpeed;
				m_Animation.Play("move");

            }
        }
	}
}
