using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public float speed = 10;
    public float attackDistance = 1;
    public float attackCooldown = 1;
    public int damage = 1;
    public AudioClip[] hitSounds;

    private float currentAttackCooldown = 0;
    private Animation animationController;


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
			animationController.Play("idle");
			GetComponent<Rigidbody>().velocity = new Vector3(0.0f, 0.0f, 0.0f);
            return;
        }

        Vector3 direction = (nearestPlayer.transform.position - transform.position).normalized;
        transform.LookAt(nearestPlayer.transform);

        if (nearestDist > attackDistance)
        {
            Vector3 velocity = direction * speed;
            GetComponent<Rigidbody>().velocity = velocity;
        }
        else if (currentAttackCooldown == 0)
        {
            currentAttackCooldown = attackCooldown;
            nearestPlayer.GetComponent<Attackable>().DealDamage(damage);

            if (animationController)
            {
                animationController["attack"].speed = 4.0f;
                animationController.Play("attack");
            }

            if (hitSounds.Length > 0)
            {
                int rndSoundIndex = Random.Range(0, hitSounds.Length);
                AudioClip rndSound = hitSounds[rndSoundIndex];
                AudioSource.PlayClipAtPoint(rndSound, transform.position, 0.5f);
            }
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
