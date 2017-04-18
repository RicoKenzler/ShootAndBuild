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
        transform.Translate(delta * Direction);
    }

    void OnTriggerEnter(Collider other)
    {
        // we are immune to our own projectiles
        if (owner && (owner.gameObject == other.gameObject))
		{
			return;
		}

		Attackable targetAttackable = other.GetComponent<Attackable>();

		if (targetAttackable && (targetAttackable.faction == ownerFaction))
		{
			// no friendly fire
			return;
		}
		
        Destroy(gameObject);

        if (targetAttackable != null)
        {
            targetAttackable.DealDamage(Damage, gameObject);
        }
        
    }

    public Vector3 Direction
    {
        get; set;
    }

	public int Damage
    {
        get; set;
    }

	private Shootable	owner;
	private Faction		ownerFaction;	//< remember faction separately as Owner could have died when we need the info

    public Shootable Owner
    {
        get
		{
			return owner;
		}

		set
		{
			owner = value;
			ownerFaction = owner.GetComponent<Attackable>().faction;
		}
    }
}
