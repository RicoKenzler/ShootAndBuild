using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
	[SerializeField] Image healthBarFillImage;
	[SerializeField] Image activeItemImage;
	[SerializeField] Image activeWeaponImage;
	[SerializeField] Image activeBuildingImage;
	[SerializeField] Text  activeItemCountText;
	[SerializeField] Animator weaponSelectionRect;
	[SerializeField] Animator itemSelectionRect;
	[SerializeField] Animator buildingSelectionRect;

	private int							displayedHealthText					= 0;
	private float						displayedHealthRelative				= 0.0f;
	bool								displayedPlayerAlive				= false;
	private int							displayedActiveItemCount			= -1;
	private ItemType					displayedActiveItemType				= ItemType.None;
	private	InventorySelectionCategory	displayedActiveSelectionCategory	= InventorySelectionCategory.Item;
	private	float						lastMenuInteraction					= 0.0f;

	public bool			useDynamicHealthColor		= false;
	public float		healthBarSmoothness			= 0.8f;
	public float		timeUntilSelectionFadeout	= 1.5f;

	private Attackable	assignedAttackable;
	private Inventory	assignedInventory;
	private Animator	activeItemCountTextAnimator;
	private Animator	activeItemImageAnimator;

	private Color deactivatedColorTint = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	private Color activatedColorTint   = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	private Color defaultTextColor;

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
		UpdateInventorySelection();
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
			UpdateInventorySelection(true);
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
		ItemData itemData = ItemManager.instance.GetItemInfos(activeItemType);

		int activeItemCount = assignedInventory.GetItemCount(assignedInventory.activeItemType);

		bool itemTypeChanged = (displayedActiveItemType != activeItemType);
		if (forceUpdateAll || itemTypeChanged)
		{
			// Update Active item Type
			displayedActiveItemType		= activeItemType;
			activeItemImage.overrideSprite = itemData.icon;
		}

		bool itemCountChanged = (displayedActiveItemCount != activeItemCount);
		if (forceUpdateAll || itemTypeChanged || itemCountChanged)
		{
			// Update Item Count
			activeItemCountText.text = activeItemCount.ToString();

			if (activeItemCount > displayedActiveItemCount && activeItemCount > 0)
			{
				HighlightActiveItemCount();
			}

			displayedActiveItemCount = activeItemCount;
		}

		bool deactivatedItem = (!IsPlayerAlive() || (activeItemCount == 0));
		
		activeItemImage.color		= deactivatedItem ? deactivatedColorTint : activatedColorTint;
		activeItemCountText.color	= deactivatedItem ? deactivatedColorTint : defaultTextColor;
		activeWeaponImage.color		= IsPlayerAlive() ? activatedColorTint : deactivatedColorTint;
		activeBuildingImage.color   = IsPlayerAlive() ? activatedColorTint : deactivatedColorTint;
	}

	public void HighlightActiveItem()
	{
		activeItemImageAnimator.SetTrigger("Grow");
	}

	public void HighlightActiveItemCount()
	{
		activeItemCountTextAnimator.SetTrigger("Grow");
	}

	void UpdateInventorySelection(bool forceUpdate = false)
	{
		InventorySelectionCategory newCategory = assignedInventory.activeSelectionCategory;

		bool forceSelectionHide = false;

		if ((lastMenuInteraction != 0.0f) && (Time.time > (lastMenuInteraction + timeUntilSelectionFadeout)))
		{
			forceSelectionHide  = true;
			lastMenuInteraction = 0.0f;
		}

		if (!forceSelectionHide && !forceUpdate && (displayedActiveSelectionCategory == newCategory))
		{
			return;
		}

		Animator oldSelectionRect = GetSelectionRectForCategory(displayedActiveSelectionCategory);
		Animator newSelectionRect = GetSelectionRectForCategory(newCategory);

		oldSelectionRect.SetBool("Visible", false);
		newSelectionRect.SetBool("Visible", (!IsPlayerAlive() || forceSelectionHide) ? false : true);

		displayedActiveSelectionCategory	= newCategory;
		lastMenuInteraction					= Time.time;
	}

	Animator GetSelectionRectForCategory(InventorySelectionCategory category)
	{
		switch (category)
		{
			case InventorySelectionCategory.Item:
				return itemSelectionRect;
			case InventorySelectionCategory.Weapon:
				return weaponSelectionRect;
			case InventorySelectionCategory.Building:
				return buildingSelectionRect;
		}

		Debug.LogWarning("Missing case statement");
		return null;
	}

	public void AssignPlayer(GameObject player)
	{
		assignedAttackable			= player.GetComponent<Attackable>();
		assignedInventory			= player.GetComponent<Inventory>();
		activeItemCountTextAnimator = activeItemCountText.GetComponent<Animator>();
		activeItemImageAnimator		= activeItemImage.GetComponent<Animator>();

		defaultTextColor			= activeItemCountText.color;

		UpdateUI();
	}
}
