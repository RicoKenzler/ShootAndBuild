using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Speed in units per second")]
    public float Speed = 5;

	// Use this for initialization
	void Start()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        float delta = Time.deltaTime * Speed;
        transform.Translate(delta * Direction);
	}

    void OnTriggerEnter(Collider other)
    {
        // we are immune to our own projectiles
        if (Owner.gameObject != other.gameObject)
        {
            Destroy(gameObject);

            Attackable attackable = other.GetComponent<Attackable>();
            if (attackable != null)
            {
                attackable.DealDamage(1);
            }
        }
    }

    public Vector3 Direction
    {
        get; set;
    }

    public Shootable Owner
    {
        get; set;
    }
}
