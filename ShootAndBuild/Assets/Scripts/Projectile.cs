using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Speed in units per second")]
    public float speed = 5;


    void Start()
    {
    }

    void Update()
    {
        float delta = Time.deltaTime * speed;
        transform.Translate(delta * direction);
    }

    void OnTriggerEnter(Collider other)
    {
        // we are immune to our own projectiles
        if (owner && (owner.gameObject == other.gameObject))
		{
			return;
		}

		Attackable targetAttackable = other.GetComponent<Attackable>();
		Attackable ownerAttackable  = owner.GetComponent<Attackable>();

		if (targetAttackable && (targetAttackable.faction == ownerAttackable.faction))
		{
			// no friendly fire
			return;
		}
		
        Destroy(gameObject);

        if (targetAttackable != null)
        {
            targetAttackable.DealDamage(damage, gameObject);
        }
        
    }

    public Vector3 direction
    {
        get; set;
    }

	public int damage
    {
        get; set;
    }

    public Shootable owner
    {
        get; set;
    }
}
