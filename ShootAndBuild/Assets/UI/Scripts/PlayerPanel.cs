using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using UnityEngine;

public class PlayerPanel : MonoBehaviour
{
	[SerializeField]
	Image healthBarFillImage;

	[SerializeField]
	Image activeItemImage;

	[SerializeField]
	Image activeWeaponImage;

	private int		displayedHealthText			= 0;
	private float	displayedHealthRelative		= 0.0f;
	bool			displayedPlayerAlive		= false;

	private GameObject activeItem;
	private GameObject activeWeapon;

	private Attackable assignedAttackable;

	// Update is called once per frame
	void Update ()
	{
		UpdateUI();
	}

	void UpdateUI()
	{
		UpdateHealthBar();
		UpdateIsPlayerAlive();
	}

	void UpdateIsPlayerAlive()
	{
		bool isPlayerAlive = assignedAttackable.health > 0;

		if (isPlayerAlive != displayedPlayerAlive)
		{
			// Do not interpolate
			UpdateHealthBar(true);

			Color indicatorColor = isPlayerAlive ? new Color(1.0f, 1.0f, 1.0f) : new Color(0.5f, 0.5f, 0.5f, 0.5f);
			activeItemImage.color	= indicatorColor;
			activeWeaponImage.color = indicatorColor;
		}
	}

	private void UpdateHealthBar(bool forceImmediateUpdate = false)
	{
		int   newHealth			= assignedAttackable.health;
		float newHealthRelative = (float) assignedAttackable.health / (float) assignedAttackable.maxHealth;

		if (!forceImmediateUpdate && displayedHealthText == newHealth && newHealthRelative == displayedHealthRelative)
		{
			return;
		}

		float healthDifference = newHealthRelative - displayedHealthRelative;

		const float HEALTH_DELTA = 0.01f;
		float healthDelta = healthDifference > 0 ? HEALTH_DELTA : -HEALTH_DELTA;

		float smothRelativeHealth = displayedHealthRelative + healthDelta;

		if (forceImmediateUpdate || Mathf.Abs(healthDifference) <= healthDelta)
		{
			smothRelativeHealth = newHealthRelative;
		}

		Color colorFullHealth =     new Color(0.0f, 1.0f, 0.0f, 1.0f);
        Color colorMediumHealth =   new Color(1.0f, 1.0f, 0.0f, 1.0f);
        Color colorNoHealth =       new Color(1.0f, 0.0f, 0.0f, 1.0f);

        Color desiredColor;
        if (smothRelativeHealth < 0.5f)
        {
            desiredColor = Color.Lerp(colorNoHealth, colorMediumHealth, smothRelativeHealth * 2.0f);
        }
        else
        {
            desiredColor = Color.Lerp(colorMediumHealth, colorFullHealth, (smothRelativeHealth - 0.5f) * 2.0f);
        }
		
		healthBarFillImage.color		= desiredColor;
		healthBarFillImage.fillAmount	= smothRelativeHealth;

		displayedHealthRelative = smothRelativeHealth;
		displayedHealthText		= newHealth;
	}

	public void AssignPlayer(GameObject player)
	{
		assignedAttackable = player.GetComponent<Attackable>();

		UpdateUI();
	}
}
