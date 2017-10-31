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
        [SerializeField] private Animator	m_WeaponSelectionRect;
        [SerializeField] private Animator	m_ItemSelectionRect;
        [SerializeField] private Animator	m_BuildingSelectionRect;

        [SerializeField] private bool	m_UseDynamicHealthColor		= false;
        [SerializeField] private float	m_HealthBarSmoothness		= 0.8f;
        [SerializeField] private float	m_TimeUntilSelectionFadeout	= -1;

		///////////////////////////////////////////////////////////////////////////

        private int			m_DisplayedHealthText		= 0;
        private float		m_DisplayedHealthRelative	= 0.0f;
        private bool		m_DisplayedPlayerAlive		= false;
        private int			m_DisplayedActiveItemCount	= -1;
        private ItemType	m_DisplayedActiveItemType	= ItemType.None;
        private Building	m_DisplayedActiveBuilding	= null;
		private WeaponData	m_DisplayedActiveWeapon		= null;
        private InventorySelectionCategory m_DisplayedActiveSelectionCategory = InventorySelectionCategory.Item;

        private Attackable	m_AssignedAttackable;
        private Inventory	m_AssignedInventory;
        private PlayerMenu	m_AssignedPlayerMenu;

        private Animator	m_ActiveItemCountTextAnimator;
        private Animator	m_ActiveItemImageAnimator;

        private Color		m_DefaultItemCountTextColor;

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
            UpdateInventorySelection();
            UpdateBuildings();
			UpdateWeapons();
        }

		///////////////////////////////////////////////////////////////////////////

        bool IsPlayerAlive()
        {
            return m_AssignedAttackable.Health > 0;
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
                UpdateInventorySelection();
                m_DisplayedPlayerAlive = isPlayerAlive;
            }
        }

		///////////////////////////////////////////////////////////////////////////

        private void UpdateHealthBar(bool forceImmediateUpdate = false)
        {
            int newHealth = m_AssignedAttackable.Health;
            float newHealthRelative = (float)m_AssignedAttackable.Health / (float)m_AssignedAttackable.maxHealth;

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

            m_HealthBarFillImage.color = desiredColor;
            m_HealthBarFillImage.fillAmount = smoothRelativeHealth;

            m_DisplayedHealthRelative = smoothRelativeHealth;
            m_DisplayedHealthText = newHealth;
        }

		///////////////////////////////////////////////////////////////////////////

        void UpdateBuildings(bool forceUpdateAll = false)
        {
            Building activeBuilding = m_AssignedPlayerMenu.activeBuildingPrefab;

            bool buildingTypeChanged = (m_DisplayedActiveBuilding != activeBuilding);
            if (forceUpdateAll || buildingTypeChanged)
            {
                // Update Active item Type
                m_DisplayedActiveBuilding = activeBuilding;
                m_ActiveBuildingImage.overrideSprite = activeBuilding ? activeBuilding.icon : null;
            }

            bool buildingBuildable = (IsPlayerAlive() && (activeBuilding && activeBuilding.IsPayable()));

            m_ActiveBuildingImage.color = buildingBuildable ? ACTIVATED_COLOR_TINT : DEACTIVATED_COLOR_TINT;
        }

		///////////////////////////////////////////////////////////////////////////

		void UpdateWeapons(bool forceUpdateAll = false)
        {
            WeaponData activeWeapon = m_AssignedPlayerMenu.activeWeapon;

            bool weaponChanged = (m_DisplayedActiveWeapon != activeWeapon);
            if (forceUpdateAll || weaponChanged)
            {
                // Update Active item Type
                m_DisplayedActiveWeapon = activeWeapon;

				if (activeWeapon)
				{
					ItemType itemType = activeWeapon.weaponID;

					ItemData weaponItemInfos = ItemManager.instance.GetItemInfos(itemType);

					m_ActiveWeaponImage.overrideSprite = weaponItemInfos.icon;
				}
				else
				{
					m_ActiveWeaponImage.overrideSprite = null;
				}
            }

            bool weaponUsable = (IsPlayerAlive() && activeWeapon && (activeWeapon.Cooldown < 0.1f));

            m_ActiveWeaponImage.color = weaponUsable ? ACTIVATED_COLOR_TINT : DEACTIVATED_COLOR_TINT;
        }

		///////////////////////////////////////////////////////////////////////////

        void UpdateItems(bool forceUpdateAll = false)
        {
            ItemType activeItemType = m_AssignedPlayerMenu.activeItemType;
            ItemData itemData = ItemManager.instance.GetItemInfos(activeItemType);

            int activeItemCount = m_AssignedInventory.GetItemCount(m_AssignedPlayerMenu.activeItemType);

            bool itemTypeChanged = (m_DisplayedActiveItemType != activeItemType);
            if (forceUpdateAll || itemTypeChanged)
            {
                // Update Active item Type
                m_DisplayedActiveItemType = activeItemType;
                m_ActiveItemImage.overrideSprite = itemData.icon;
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
            m_ActiveWeaponImage.color = IsPlayerAlive() ? ACTIVATED_COLOR_TINT : DEACTIVATED_COLOR_TINT;
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

        void UpdateInventorySelection()
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
            m_AssignedAttackable = player.GetComponent<Attackable>();
            m_AssignedInventory = player.GetComponent<Inventory>();
            m_AssignedPlayerMenu = player.GetComponent<PlayerMenu>();

            m_ActiveItemCountTextAnimator = m_ActiveItemCountText.GetComponent<Animator>();
            m_ActiveItemImageAnimator = m_ActiveItemImage.GetComponent<Animator>();

            m_DefaultItemCountTextColor = m_ActiveItemCountText.color;

            UpdateUI();
        }
    }
}