using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Attackable target;
    public int yOffset = -20;
	public bool isBackgroundDuplicate = false;

    private Vector2 originalSize;
	private bool alwaysShowHealth = false;

    void Start()
    {
        originalSize		= GetComponent<RectTransform>().sizeDelta;
		alwaysShowHealth	= (target.GetComponent<InputController>() != null);
    }

    void Update()
    {
        RectTransform rect = GetComponent<RectTransform>();

        Vector2 uiPos = RectTransformUtility.WorldToScreenPoint(Camera.main, target.transform.position);
        uiPos.y += yOffset;
        rect.anchoredPosition = uiPos;

        float healthFactor = target.health / (float)target.maxHealth;

		if (!alwaysShowHealth && (healthFactor >= 1.0f))
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

		rect.sizeDelta = new Vector2(originalSize.x * healthFactor, originalSize.y);
		uiPos.x -= (originalSize.x - rect.sizeDelta.x) / 2;
		rect.anchoredPosition = uiPos;
		

        Color colorFullHealth =     new Color(0.0f, 1.0f, 0.0f, 1.0f);
        Color colorMediumHealth =   new Color(1.0f, 1.0f, 0.0f, 1.0f);
        Color colorNoHealth =       new Color(1.0f, 0.0f, 0.0f, 1.0f);

        Color desiredColor;
        if (healthFactor < 0.5f)
        {
            desiredColor = Color.Lerp(colorNoHealth, colorMediumHealth, healthFactor * 2.0f);
        }
        else
        {
            desiredColor = Color.Lerp(colorMediumHealth, colorFullHealth, (healthFactor - 0.5f) * 2.0f);
        }

        GetComponent<Image>().color = desiredColor;
    }

}
