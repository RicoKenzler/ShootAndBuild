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
        if (owner.gameObject != other.gameObject)
        {
            Destroy(gameObject);

            Attackable attackable = other.GetComponent<Attackable>();
            if (attackable != null)
            {
                attackable.DealDamage(1, gameObject);
            }
        }
    }

    public Vector3 direction
    {
        get; set;
    }

    public Shootable owner
    {
        get; set;
    }
}
