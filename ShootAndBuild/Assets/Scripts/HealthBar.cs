using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Attackable target;
    public int yOffset = -20;
	public bool isBackgroundDuplicate = false;
	public float healthSmoothAmount = 0.9f;
	public bool hasDynamicColor = false;

    private Vector2 originalSize;
	private bool alwaysShowHealth = false;
	private float lastDisplayedHealthFactor;

    void Start()
    {
        originalSize				= GetComponent<RectTransform>().sizeDelta;
		lastDisplayedHealthFactor	= 1.0f;
		alwaysShowHealth			= (target.GetComponent<InputController>() != null);
    }

    void Update()
    {
        RectTransform rect = GetComponent<RectTransform>();

        Vector2 uiPos = RectTransformUtility.WorldToScreenPoint(Camera.main, target.transform.position);
        uiPos.y += yOffset;
        rect.anchoredPosition = uiPos;

        float exactHealthFactor		= target.health / (float)target.maxHealth;
		float smoothedHealthFactor	= Mathf.Lerp(lastDisplayedHealthFactor, exactHealthFactor, 1.0f - healthSmoothAmount);

		lastDisplayedHealthFactor = smoothedHealthFactor;

		if (!alwaysShowHealth && (smoothedHealthFactor >= 0.999f))
		{
			// Full health: No Bar
			rect.sizeDelta = new Vector2(0.0f, 0.0f);
			return;
		}
		
		if (isBackgroundDuplicate)
		{
			GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
			rect.sizeDelta = originalSize;
			return;
		}


		Vector2 desiredSize = new Vector2(originalSize.x * smoothedHealthFactor, originalSize.y);
		
		rect.sizeDelta = desiredSize;

		uiPos.x -= (originalSize.x - rect.sizeDelta.x) / 2;
		rect.anchoredPosition = uiPos;
		
		

		if (hasDynamicColor)
		{
			Color desiredColor;
			Color colorFullHealth =     new Color(0.0f, 1.0f, 0.0f, 1.0f);
			Color colorMediumHealth =   new Color(1.0f, 1.0f, 0.0f, 1.0f);
			Color colorNoHealth =       new Color(1.0f, 0.0f, 0.0f, 1.0f);

        
			if (smoothedHealthFactor < 0.5f)
			{
				desiredColor = Color.Lerp(colorNoHealth, colorMediumHealth, smoothedHealthFactor * 2.0f);
			}
			else
			{
				desiredColor = Color.Lerp(colorMediumHealth, colorFullHealth, (smoothedHealthFactor - 0.5f) * 2.0f);
			}

			GetComponent<Image>().color = desiredColor;
		}
    }

}
