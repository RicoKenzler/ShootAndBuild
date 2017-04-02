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

	public float	healthBarSmoothness			= 0.8f;

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

			displayedPlayerAlive = isPlayerAlive;
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
	
		// Method 2: start fast
		float smoothRelativeHealth = Mathf.Lerp(newHealthRelative, displayedHealthRelative, healthBarSmoothness);

		if (forceImmediateUpdate || Mathf.Abs(healthDifference) <= 0.001f)
		{
			smoothRelativeHealth = newHealthRelative;
		}

		Color colorFullHealth =     new Color(0.0f, 1.0f, 0.0f, 1.0f);
        Color colorMediumHealth =   new Color(1.0f, 1.0f, 0.0f, 1.0f);
        Color colorNoHealth =       new Color(1.0f, 0.0f, 0.0f, 1.0f);

        Color desiredColor;
        if (smoothRelativeHealth < 0.5f)
        {
            desiredColor = Color.Lerp(colorNoHealth, colorMediumHealth, smoothRelativeHealth * 2.0f);
        }
        else
        {
            desiredColor = Color.Lerp(colorMediumHealth, colorFullHealth, (smoothRelativeHealth - 0.5f) * 2.0f);
        }
		
		healthBarFillImage.color		= desiredColor;
		healthBarFillImage.fillAmount	= smoothRelativeHealth;

		displayedHealthRelative = smoothRelativeHealth;
		displayedHealthText		= newHealth;
	}

	public void AssignPlayer(GameObject player)
	{
		assignedAttackable = player.GetComponent<Attackable>();

		UpdateUI();
	}
}
