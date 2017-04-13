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

	[SerializeField]
	Text  activeItemCountText;

	private int			displayedHealthText			= 0;
	private float		displayedHealthRelative		= 0.0f;
	bool				displayedPlayerAlive		= false;
	private int			displayedActiveItemCount	= -1;
	private ItemType	displayedActiveItemType		= ItemType.None;

	public bool     useDynamicHealthColor		= false;
	public float	healthBarSmoothness			= 0.8f;

	private Attackable	assignedAttackable;
	private Inventory	assignedInventory;
	private Animator	activeItemCountTextAnimator;

	private Color deactivatedColorTint = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	private Color activatedColorTint   = new Color(1.0f, 1.0f, 1.0f, 1.0f);

	// Update is called once per frame
	void Update ()
	{
		UpdateUI();
	}

	void UpdateUI()
	{
		if (!assignedAttackable)
		{
			// for testing standalone version
			return;
		}

		UpdateHealthBar();
		UpdateIsPlayerAlive();
		UpdateItems();
	}

	bool IsPlayerAlive()
	{
		return assignedAttackable.health > 0;
	}

	void UpdateIsPlayerAlive()
	{
		bool isPlayerAlive = IsPlayerAlive();

		if (isPlayerAlive != displayedPlayerAlive)
		{
			// Do not interpolate
			UpdateHealthBar(true);
			UpdateItems(true);
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

		Color desiredColor = new Color(1.0f, 0.0f, 0.0f);

		if (useDynamicHealthColor)
		{
			Color colorFullHealth =     new Color(0.0f, 1.0f, 0.0f, 1.0f);
			Color colorMediumHealth =   new Color(1.0f, 1.0f, 0.0f, 1.0f);
			Color colorNoHealth =       new Color(1.0f, 0.0f, 0.0f, 1.0f);

			if (smoothRelativeHealth < 0.5f)
			{
				desiredColor = Color.Lerp(colorNoHealth, colorMediumHealth, smoothRelativeHealth * 2.0f);
			}
			else
			{
				desiredColor = Color.Lerp(colorMediumHealth, colorFullHealth, (smoothRelativeHealth - 0.5f) * 2.0f);
		   }
		}
		
		
		healthBarFillImage.color		= desiredColor;
		healthBarFillImage.fillAmount	= smoothRelativeHealth;

		displayedHealthRelative = smoothRelativeHealth;
		displayedHealthText		= newHealth;
	}

	void UpdateItems(bool forceUpdateAll = false)
	{
		ItemType activeItemType = assignedInventory.activeItemType;
		int activeItemCount = assignedInventory.GetItemCount(assignedInventory.activeItemType);

		bool forceItemCountUpdate = false;

		bool itemTypeChanged = (displayedActiveItemType != activeItemType);
		if (forceUpdateAll || itemTypeChanged)
		{
			// Update Active item Type
			displayedActiveItemType = activeItemType;
			forceItemCountUpdate = true;
		}

		bool itemCountChanged = (displayedActiveItemCount != activeItemCount);
		if (forceUpdateAll || itemTypeChanged || itemCountChanged)
		{
			// Update Item Count
			activeItemCountText.text = activeItemCount.ToString();

			if (itemCountChanged)
			{
				activeItemCountTextAnimator.SetTrigger("Grow");
			}

			displayedActiveItemCount = activeItemCount;
		}

		Color activeItemColor = (!IsPlayerAlive() || (activeItemCount == 0)) ? deactivatedColorTint : activatedColorTint;
		
		activeItemImage.color	= activeItemColor;
		activeWeaponImage.color = IsPlayerAlive() ? activatedColorTint : deactivatedColorTint;
	}

	public void AssignPlayer(GameObject player)
	{
		assignedAttackable = player.GetComponent<Attackable>();
		assignedInventory  = player.GetComponent<Inventory>();
		activeItemCountTextAnimator = activeItemCountText.GetComponent<Animator>();

		UpdateUI();
	}
}
