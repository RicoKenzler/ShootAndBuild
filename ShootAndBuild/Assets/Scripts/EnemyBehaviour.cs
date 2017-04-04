using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public float		speed = 10;
    public float		attackDistance = 1;
    public float		attackCooldown = 1;
    public int			damage = 1;
    public AudioClip[]	hitSounds;

    private float		currentAttackCooldown = 0;
    private Animation	animationController;

    void Start()
    {
        animationController = GetComponentInChildren<Animation>();
        if (animationController == null)
        {
            Debug.LogWarning("no animation found on enemy");
        }
        else
        {
            animationController["idle"].speed = 1;
            animationController.Play();
        }
    }

    void Update()
    {
        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown = Mathf.Max(currentAttackCooldown - Time.deltaTime, 0);
        }

        GameObject nearestPlayer = PlayerManager.instance.GetNearestPlayer(transform.position);

        if (!nearestPlayer)
        {
			if (!animationController.IsPlaying("attack"))
			{
				animationController.Play("idle");
			}

			GetComponent<Rigidbody>().velocity = new Vector3(0.0f, 0.0f, 0.0f);
            return;
        }

        Vector3 direction = (nearestPlayer.transform.position - transform.position);
		float distToPlayer = direction.magnitude;

		if (distToPlayer == 0.0f)
		{
			direction    = new Vector3(1.0f, 0.0f, 0.0f);
			distToPlayer = 1.0f;
		}
		else
		{
			direction /= distToPlayer;
		}

        transform.LookAt(nearestPlayer.transform);

        if (distToPlayer > attackDistance)
        {
            Vector3 velocity = direction * speed;
            GetComponent<Rigidbody>().velocity = velocity;
        }
        else if (currentAttackCooldown == 0)
        {
            currentAttackCooldown = attackCooldown;
            nearestPlayer.GetComponent<Attackable>().DealDamage(damage, gameObject);

            if (animationController)
            {
                animationController["attack"].speed = 4.0f;
                animationController.Play("attack");
            }

            AudioManager.instance.PlayRandomOneShot(hitSounds, new OneShotParams(transform.position));
        }


        /////////////////////////////////////////
        // Animation
        /////////////////////////////////////////
        if (animationController != null)
        {
            float movementSpeed = 1.0f;

            if (!animationController.IsPlaying("attack"))
            {
                animationController["walk"].speed = movementSpeed;
                animationController.Play("walk");

            }
        }
    }
}
