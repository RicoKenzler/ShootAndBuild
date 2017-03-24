using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shootable : MonoBehaviour
{
	public AudioClip[] ShootSounds;
    public GameObject ProjectilePrefab;
    public float ShootCooldown = 0.5f;

    public float FlashDuration      = 0.5f;
    public float FlashMaxIntensity  = 0.5f;

    private float m_CurrentCooldown         = 0.0f;
    private float m_DefaultLightIntensity   = 0.0f;
    private float m_LastShootTime           = -1000.0f;

    Light playerLight;

    // Use this for initialization
    void Start ()
    {
        playerLight = GetComponentInChildren<Light>();
        m_DefaultLightIntensity = playerLight.intensity;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (m_CurrentCooldown > 0.0f)
        {
            m_CurrentCooldown = Mathf.Max(m_CurrentCooldown - Time.deltaTime, 0.0f);
        }

        float timeSinceLastShot = Time.time - m_LastShootTime;

        float flashIntensity = 1.0f - (timeSinceLastShot / FlashDuration);
        flashIntensity *= FlashMaxIntensity;

        flashIntensity = Mathf.Max(flashIntensity, 0.0f);

        flashIntensity = flashIntensity * flashIntensity;

        playerLight.intensity = m_DefaultLightIntensity + flashIntensity;
    }

    public void Shoot()
    {
        if (m_CurrentCooldown > 0.0f)
        {
            return;
        }

        GameObject projectileContainer = GameObject.Find("Projectiles");

        GameObject instance = Instantiate(ProjectilePrefab, projectileContainer.transform);
        instance.transform.position = transform.position;

        Projectile projectile = instance.GetComponent<Projectile>();
        projectile.Direction = transform.forward;
        projectile.Owner = this;

        m_CurrentCooldown   = ShootCooldown;
        m_LastShootTime     = Time.time;

		if (ShootSounds.Length > 0)
		{
			int rndSoundIndex = Random.Range(0, ShootSounds.Length -1);
			AudioClip rndSound = ShootSounds[rndSoundIndex];
			AudioSource.PlayClipAtPoint(rndSound, transform.position, 0.5f);
		}
    }
}
