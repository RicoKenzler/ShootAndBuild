using UnityEngine;
using UnityEngine.UI;

namespace SAB
{
    public class UiBar : MonoBehaviour
    {
        public Image BarFillingImage;
        private RectTransform BarFillingTransform;

        [Range(0.0f, 1.0f)]
        public float TargetPercentage = 0.5f;

        [Range(0.0f, 0.999f)]
        public float Smoothness = 0.85f;

        float CurrentPercentage  = 0.5f;

        private void Awake()
        {
            BarFillingTransform = BarFillingImage.rectTransform;
        }

        // Use this for initialization
        void Start ()
        {
		
	    }
	
	    // Update is called once per frame
	    void Update ()
        {
		    UpdateBarIfNecessary();
	    }

        void UpdateBarIfNecessary()
        {
            if (TargetPercentage == CurrentPercentage)
            {
                return;
            }

            CurrentPercentage = Mathf.Lerp(TargetPercentage, CurrentPercentage, Smoothness);

            if (Mathf.Abs(CurrentPercentage - TargetPercentage) < 0.005f)
            {
                CurrentPercentage = TargetPercentage;
            }

            UpdateBar();
        }

        void UpdateBar()
        {
            BarFillingTransform.anchorMin = new Vector2(CurrentPercentage - 1.0f, BarFillingTransform.anchorMin.y);
            BarFillingTransform.anchorMax = new Vector2(CurrentPercentage, BarFillingTransform.anchorMax.y);
        }

    }
}