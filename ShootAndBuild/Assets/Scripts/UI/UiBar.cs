using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SAB
{
    public class UiBar : MonoBehaviour
    {
		[FormerlySerializedAs("BarFillingImage")]
        [SerializeField] private Image m_BarFillingImage;

		[FormerlySerializedAs("TargetPercentage")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float m_TargetPercentage = 0.5f;

		[FormerlySerializedAs("Smoothness")]
        [Range(0.0f, 0.999f)]
        [SerializeField] private float m_Smoothness = 0.85f;

		///////////////////////////////////////////////////////////////////////////

        private RectTransform m_BarFillingTransform;
        private float m_CurrentPercentage  = 0.5f;

		///////////////////////////////////////////////////////////////////////////

		public float targetPercentage { get { return m_TargetPercentage; } set { m_TargetPercentage = value; } }

		///////////////////////////////////////////////////////////////////////////

        private void Awake()
        {
            m_BarFillingTransform = m_BarFillingImage.rectTransform;
        }

		///////////////////////////////////////////////////////////////////////////
	
	    // Update is called once per frame
	    void Update ()
        {
		    UpdateBarIfNecessary();
	    }

		///////////////////////////////////////////////////////////////////////////

        void UpdateBarIfNecessary()
        {
            if (m_TargetPercentage == m_CurrentPercentage)
            {
                return;
            }

            m_CurrentPercentage = Mathf.Lerp(m_TargetPercentage, m_CurrentPercentage, m_Smoothness);

            if (Mathf.Abs(m_CurrentPercentage - m_TargetPercentage) < 0.005f)
            {
                m_CurrentPercentage = m_TargetPercentage;
            }

            UpdateBar();
        }

		///////////////////////////////////////////////////////////////////////////

        void UpdateBar()
        {
            m_BarFillingTransform.anchorMin = new Vector2(m_CurrentPercentage - 1.0f, m_BarFillingTransform.anchorMin.y);
            m_BarFillingTransform.anchorMax = new Vector2(m_CurrentPercentage, m_BarFillingTransform.anchorMax.y);
        }
    }
}