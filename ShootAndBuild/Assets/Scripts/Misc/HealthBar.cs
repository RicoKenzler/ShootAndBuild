using UnityEngine;
using UnityEngine.UI;

namespace SAB
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private int		m_yOffset				= -20;
        [SerializeField] private bool		m_IsBackgroundDuplicate = false;
        [SerializeField] private float		m_HealthSmoothAmount	= 0.9f;
        [SerializeField] private bool		m_HasDynamicColor		= false;

		///////////////////////////////////////////////////////////////////////////

        private bool		m_AlwaysShowHealth = false;
        private Attackable	m_Target;
        private Vector2		m_OriginalSize;
        private float		m_LastDisplayedHealthFactor;
        private Image		m_Image;

		///////////////////////////////////////////////////////////////////////////

		public bool			isBackgroundDuplicate	{ get { return m_IsBackgroundDuplicate; }}
		public Attackable	target					{ get { return m_Target; } }
		
		///////////////////////////////////////////////////////////////////////////

		public void Init(Attackable target, bool isBackgroundDuplicate)
		{
			m_Target				= target;
			m_IsBackgroundDuplicate = isBackgroundDuplicate;
		}

		///////////////////////////////////////////////////////////////////////////

        void Start()
        {
            m_OriginalSize = GetComponent<RectTransform>().sizeDelta;
            m_Image = GetComponent<Image>();
            m_LastDisplayedHealthFactor = 1.0f;
            m_AlwaysShowHealth = (m_Target.GetComponent<InputController>() != null);
        }

		///////////////////////////////////////////////////////////////////////////

        void Update()
        {
            RectTransform rect = GetComponent<RectTransform>();

            Vector2 uiPos = RectTransformUtility.WorldToScreenPoint(Camera.main, m_Target.transform.position);
            uiPos.y += m_yOffset;
            rect.anchoredPosition = uiPos;

            float exactHealthFactor = m_Target.Health / (float)m_Target.maxHealth;
            float smoothedHealthFactor = Mathf.Lerp(m_LastDisplayedHealthFactor, exactHealthFactor, 1.0f - m_HealthSmoothAmount);

            m_LastDisplayedHealthFactor = smoothedHealthFactor;

            if (!m_AlwaysShowHealth && (smoothedHealthFactor >= 0.999f))
            {
                // Full health: No Bar
                rect.sizeDelta = new Vector2(0.0f, 0.0f);
                return;
            }

            if (m_IsBackgroundDuplicate)
            {
                m_Image.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
                rect.sizeDelta = m_OriginalSize;
                return;
            }


            Vector2 desiredSize = new Vector2(m_OriginalSize.x * smoothedHealthFactor, m_OriginalSize.y);

            rect.sizeDelta = desiredSize;

            uiPos.x -= (m_OriginalSize.x - rect.sizeDelta.x) / 2;
            rect.anchoredPosition = uiPos;

            if (m_HasDynamicColor)
            {
                Color desiredColor;
                Color colorFullHealth = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                Color colorMediumHealth = new Color(1.0f, 1.0f, 0.0f, 1.0f);
                Color colorNoHealth = new Color(1.0f, 0.0f, 0.0f, 1.0f);

                if (smoothedHealthFactor < 0.5f)
                {
                    desiredColor = Color.Lerp(colorNoHealth, colorMediumHealth, smoothedHealthFactor * 2.0f);
                }
                else
                {
                    desiredColor = Color.Lerp(colorMediumHealth, colorFullHealth, (smoothedHealthFactor - 0.5f) * 2.0f);
                }

                m_Image.color = desiredColor;
            }
            else if (m_Target.faction == Faction.Player)
            {
                m_Image.color = new Color(0.4f, 0.7f, 0.2f, 1.0f);
            }
        }
    }
}