using UnityEngine;
using UnityEngine.UI;

namespace SAB
{

    public class PlayerPanel : MonoBehaviour
    {
        [SerializeField] private Image		m_HealthBarFillImage;

        [SerializeField] private Image		m_ActiveItemImage;
        [SerializeField] private Image		m_ActiveWeaponImage;
        [SerializeField] private Image		m_ActiveBuildingImage;

        [SerializeField] private Text		m_ActiveItemCountText;
		[SerializeField] private Text		m_ActiveWeaponAmmoCountText;
		[SerializeField] private Text		m_ActiveBuildingCostsText;

        [SerializeField] private Animator	m_WeaponSelectionRect;
        [SerializeField] private Animator	m_ItemSelectionRect;
        [SerializeField] private Animator	m_BuildingSelectionRect;

        [SerializeField] private bool	m_UseDynamicHealthColor		= false;
        [SerializeField] private float	m_HealthBarSmoothness		= 0.8f;
        [SerializeField] private float	m_TimeUntilSelectionFadeout	= -1;

		///////////////////////////////////////////////////////////////////////////

        private bool				m_DisplayedPlayerAlive		= false;

        private int					m_DisplayedHealthText		= 0;
        private float				m_DisplayedHealthRelative	= 0.0f;

        private int					m_DisplayedActiveItemCount	= -1;
		private int					m_DisplayedAmmoCount		= -1;

        private StorableItemData	m_DisplayedActiveItemType	= null;
        private Building			m_DisplayedActiveBuilding	= null;
		private WeaponData			m_DisplayedActiveWeapon		= null;

        private InventorySelectionCategory m_DisplayedActiveSelectionCategory = InventorySelectionCategory.Item;

		///////////////////////////////////////////////////////////////////////////

        private Attackable		m_AssignedAttackable;
		private InputController m_AssignedPlayer;
        private Inventory		m_AssignedInventory;
		private Shooter			m_AssignedShooter;
        private PlayerMenu		m_AssignedPlayerMenu;

		///////////////////////////////////////////////////////////////////////////

		private Animator	m_ActiveItemCountTextAnimator;
        private Animator	m_ActiveItemImageAnimator;
		private Animator	m_ActiveWeaponAmmoCountTextAnimator;
        private Animator	m_ActiveWeaponImageAnimator;
		private Animator	m_ActiveBuildingCostTextAnimator;
        private Animator	m_ActiveBuildingImageAnimator;

		///////////////////////////////////////////////////////////////////////////

        private Color		m_DefaultItemCountTextColor;
		private Color		m_DefaultAmmoCountTextColor;
		private Color		m_DefaultBuildingCostsTextColor;

		///////////////////////////////////////////////////////////////////////////

        private readonly Color DEACTIVATED_COLOR_TINT	= new Color(0.5f, 0.5f, 0.5f, 0.5f);
        private readonly Color ACTIVATED_COLOR_TINT		= new Color(1.0f, 1.0f, 1.0f, 1.0f);

		///////////////////////////////////////////////////////////////////////////

        // Update is called once per frame
        void Update()
        {
            UpdateUI();
        }

		///////////////////////////////////////////////////////////////////////////

        void UpdateUI()
        {
            if (!m_AssignedAttackable)
            {
                // for testing standalone version
                return;
            }

            UpdateHealthBar();
            UpdateIsPlayerAlive();
            UpdateItems();
            UpdateSelectionCategory();
            UpdateBuildings();
			UpdateWeapons();
        }

		///////////////////////////////////////////////////////////////////////////

        bool IsPlayerAlive()
        {
            return (m_AssignedAttackable.health > 0);
        }

		///////////////////////////////////////////////////////////////////////////

        void UpdateIsPlayerAlive()
        {
            bool isPlayerAlive = IsPlayerAlive();

            if (isPlayerAlive != m_DisplayedPlayerAlive)
            {
                // Do not interpolate
                UpdateHealthBar(true);
                UpdateItems(true);
                UpdateBuildings(true);
				UpdateWeapons(true);
                UpdateSelectionCategory();
                m_DisplayedPlayerAlive = isPlayerAlive;
            }
        }

		///////////////////////////////////////////////////////////////////////////

        private void UpdateHealthBar(bool forceImmediateUpdate = false)
        {
            int newHealth = m_AssignedAttackable.health;
            float newHealthRelative = (float)m_AssignedAttackable.health / (float)m_AssignedAttackable.maxHealth;

            if (!forceImmediateUpdate && m_DisplayedHealthText == newHealth && newHealthRelative == m_DisplayedHealthRelative)
            {
                return;
            }

            float healthDifference = newHealthRelative - m_DisplayedHealthRelative;

            // Method 2: start fast
            float smoothRelativeHealth = Mathf.Lerp(newHealthRelative, m_DisplayedHealthRelative, m_HealthBarSmoothness);

            if (forceImmediateUpdate || Mathf.Abs(healthDifference) <= 0.001f)
            {
                smoothRelativeHealth = newHealthRelative;
            }

            Color desiredColor = new Color(1.0f, 0.0f, 0.0f);

            if (m_UseDynamicHealthColor)
            {
                Color colorFullHealth = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                Color colorMediumHealth = new Color(1.0f, 1.0f, 0.0f, 1.0f);
                Color colorNoHealth = new Color(1.0f, 0.0f, 0.0f, 1.0f);

                if (smoothRelativeHealth < 0.5f)
                {
                    desiredColor = Color.Lerp(colorNoHealth, colorMediumHealth, smoothRelativeHealth * 2.0f);
                }
                else
                {
                    desiredColor = Color.Lerp(colorMediumHealth, colorFullHealth, (smoothRelativeHealth - 0.5f) * 2.0f);
                }
            }
			else
			{
				desiredColor = Color.Lerp(m_AssignedPlayer.playerColor, Color.black, 0.1f);
			}

            m_HealthBarFillImage.color = desiredColor;
            m_HealthBarFillImage.fillAmount = smoothRelativeHealth;

            m_DisplayedHealthRelative = smoothRelativeHealth;
            m_DisplayedHealthText = newHealth;
        }

		///////////////////////////////////////////////////////////////////////////

        void UpdateBuildings(bool forceUpdateAll = false)
        {
            Building activeBuilding = m_AssignedPlayerMenu.activeBuildingPrefab;

			if (!activeBuilding)
			{
				m_DisplayedActiveBuilding = null;
				m_ActiveBuildingImage.overrideSprite = null;
				m_ActiveBuildingCostsText.text = "";
				return;
			}

			PreviewImageData previewImage = activeBuilding.gameObject.GetComponent<PreviewImageData>();

            bool buildingTypeChanged = (m_DisplayedActiveBuilding != activeBuilding);
            if (forceUpdateAll || buildingTypeChanged)
            {
                // Update Active item Type
                m_DisplayedActiveBuilding = activeBuilding;
                m_ActiveBuildingImage.overrideSprite = activeBuilding ? previewImage.icon : null;

				string costString = "";

				if (activeBuilding.costs.Length != 0)
				{
					costString = activeBuilding.costs[0].count.ToString();
					costString += activeBuilding.costs[0].itemData.abbreviation;
				}

				m_ActiveBuildingCostsText.text = costString;
            }

            bool buildingBuildable = (IsPlayerAlive() && (activeBuilding && Inventory.CanBePaid(activeBuilding.costs, m_AssignedPlayer.gameObject)));

            m_ActiveBuildingImage.color		= buildingBuildable ? ACTIVATED_COLOR_TINT : DEACTIVATED_COLOR_TINT;
			m_ActiveBuildingCostsText.color = buildingBuildable ? m_DefaultBuildingCostsTextColor : DEACTIVATED_COLOR_TINT;
        }

		///////////////////////////////////////////////////////////////////////////

		void UpdateWeapons(bool forceUpdateAll = false)
        {
            Weapon activeWeapon			= m_AssignedShooter.currentWeapon;
			WeaponData activeWeaponData = activeWeapon == null ? null : activeWeapon.weaponData;

			if (!activeWeaponData)
			{
				m_DisplayedActiveWeapon = null;
				m_DisplayedAmmoCount = -1;
				m_ActiveWeaponImage.overrideSprite = null;
				m_ActiveWeaponAmmoCountText.text = "";
				return;
			}

            bool weaponChanged = (m_DisplayedActiveWeapon != activeWeaponData);

            // Weapon Changed?
            if (forceUpdateAll || weaponChanged)
            {
                m_DisplayedActiveWeapon = activeWeaponData;

				if (activeWeaponData)
				{
					PreviewImageData previewImage = activeWeaponData.gameObject.GetComponent<PreviewImageData>();
					m_ActiveWeaponImage.overrideSprite = previewImage.icon;
				}
				else
				{
					m_ActiveWeaponImage.overrideSprite = null;
				}
            }

			int ammoCount = activeWeaponData.infiniteAmmo ? int.MaxValue : activeWeapon.ammoCount;

			// Ammo Changed?
			bool ammoCountChanged = (m_DisplayedAmmoCount != ammoCount);
            if (forceUpdateAll || ammoCountChanged || weaponChanged)
            {
                // Update Item Count
                m_ActiveWeaponAmmoCountText.text = (ammoCount == int.MaxValue) ? "∞" : ammoCount.ToString();

                if ((ammoCount > m_DisplayedAmmoCount) && (ammoCount > 0))
                {
                    HightlightAmmoCount();
                }

                m_DisplayedAmmoCount = ammoCount;
            }

			bool weaponGenerallyUsable	= (IsPlayerAlive() && activeWeapon.HasEnoughAmmoToShoot());
			bool weaponNowUsable		= weaponGenerallyUsable&& (activeWeapon.cooldown < 0.1f);

            m_ActiveWeaponImage.color			= weaponNowUsable		? ACTIVATED_COLOR_TINT : DEACTIVATED_COLOR_TINT;
			m_ActiveWeaponAmmoCountText.color	= weaponGenerallyUsable ? m_DefaultAmmoCountTextColor : DEACTIVATED_COLOR_TINT;
        }

		///////////////////////////////////////////////////////////////////////////

        void UpdateItems(bool forceUpdateAll = false)
        {
            StorableItemData itemData = m_AssignedPlayerMenu.activeItemData;

			if (!itemData)
			{
				m_DisplayedActiveItemType = null;
				m_DisplayedActiveItemCount = -1;
				m_ActiveItemImage.overrideSprite = null;
				m_ActiveItemCountText.text = "";
				return;
			}

			PreviewImageData previewImage = itemData.gameObject.GetComponent<PreviewImageData>();

            int activeItemCount = m_AssignedInventory.GetItemCount(m_AssignedPlayerMenu.activeItemData);

            bool itemTypeChanged = (m_DisplayedActiveItemType != itemData);
            if (forceUpdateAll || itemTypeChanged)
            {
                // Update Active item Type
                m_DisplayedActiveItemType = itemData;
                m_ActiveItemImage.overrideSprite = previewImage.icon;
            }

            bool itemCountChanged = (m_DisplayedActiveItemCount != activeItemCount);
            if (forceUpdateAll || itemTypeChanged || itemCountChanged)
            {
                // Update Item Count
                m_ActiveItemCountText.text = activeItemCount.ToString();

                if (activeItemCount > m_DisplayedActiveItemCount && activeItemCount > 0)
                {
                    HighlightActiveItemCount();
                }

                m_DisplayedActiveItemCount = activeItemCount;
            }

            bool deactivatedItem = (!IsPlayerAlive() || (activeItemCount == 0));

            m_ActiveItemImage.color = deactivatedItem ? DEACTIVATED_COLOR_TINT : ACTIVATED_COLOR_TINT;
            m_ActiveItemCountText.color = deactivatedItem ? DEACTIVATED_COLOR_TINT : m_DefaultItemCountTextColor;
        }

		///////////////////////////////////////////////////////////////////////////

        public void HighlightActiveItem()
        {
            m_ActiveItemImageAnimator.SetTrigger("Grow");
        }

		///////////////////////////////////////////////////////////////////////////

        public void HighlightActiveItemCount()
        {
            m_ActiveItemCountTextAnimator.SetTrigger("Grow");
        }

		///////////////////////////////////////////////////////////////////////////

        public void HighlightActiveWeapon()
        {
            m_ActiveWeaponImageAnimator.SetTrigger("Grow");
        }

		///////////////////////////////////////////////////////////////////////////

        public void HightlightAmmoCount()
        {
            m_ActiveWeaponAmmoCountTextAnimator.SetTrigger("Grow");
        }

		///////////////////////////////////////////////////////////////////////////

        public void HighlightActiveBuilding()
        {
            m_ActiveBuildingImageAnimator.SetTrigger("Grow");
        }

		///////////////////////////////////////////////////////////////////////////

        public void HightlightBuildingCostCount()
        {
            m_ActiveBuildingCostTextAnimator.SetTrigger("Grow");
        }

		///////////////////////////////////////////////////////////////////////////

        void UpdateSelectionCategory()
        {
            InventorySelectionCategory newCategory = m_AssignedPlayerMenu.activeSelectionCategory;

            bool hideSelection = false;

            if (m_TimeUntilSelectionFadeout >= 0.0f)
            {
                if ((Time.time > (m_AssignedPlayerMenu.lastMenuInteractionTime + m_TimeUntilSelectionFadeout)))
                {
                    hideSelection = true;
                }
            }

            Animator oldSelectionRect = GetSelectionRectForCategory(m_DisplayedActiveSelectionCategory);
            Animator newSelectionRect = GetSelectionRectForCategory(newCategory);

            oldSelectionRect.SetBool("Visible", false);
            newSelectionRect.SetBool("Visible", (!IsPlayerAlive() || hideSelection) ? false : true);

            m_DisplayedActiveSelectionCategory = newCategory;
        }

		///////////////////////////////////////////////////////////////////////////

        Animator GetSelectionRectForCategory(InventorySelectionCategory category)
        {
            switch (category)
            {
                case InventorySelectionCategory.Item:
                    return m_ItemSelectionRect;
                case InventorySelectionCategory.Weapon:
                    return m_WeaponSelectionRect;
                case InventorySelectionCategory.Building:
                    return m_BuildingSelectionRect;
            }

            Debug.LogWarning("Missing case statement");
            return null;
        }

		///////////////////////////////////////////////////////////////////////////

        public void AssignPlayer(GameObject player)
        {
            m_AssignedAttackable	= player.GetComponent<Attackable>();
            m_AssignedInventory		= player.GetComponent<Inventory>();
            m_AssignedPlayerMenu	= player.GetComponent<PlayerMenu>();
			m_AssignedPlayer		= player.GetComponent<InputController>();
			m_AssignedShooter		= player.GetComponent<Shooter>();

            m_ActiveItemCountTextAnimator		= m_ActiveItemCountText.GetComponent<Animator>();
            m_ActiveItemImageAnimator			= m_ActiveItemImage.GetComponent<Animator>();
			m_ActiveBuildingCostTextAnimator	= m_ActiveBuildingCostsText.GetComponent<Animator>();
			m_ActiveBuildingImageAnimator		= m_ActiveBuildingImage.GetComponent<Animator>();
			m_ActiveWeaponAmmoCountTextAnimator	= m_ActiveWeaponAmmoCountText.GetComponent<Animator>();
			m_ActiveWeaponImageAnimator			= m_ActiveWeaponImage.GetComponent<Animator>();

            m_DefaultItemCountTextColor		= m_ActiveItemCountText.color;
			m_DefaultAmmoCountTextColor		= m_ActiveWeaponAmmoCountText.color;
			m_DefaultBuildingCostsTextColor	= m_ActiveBuildingCostsText.color;

            UpdateUI();
        }
    }
}