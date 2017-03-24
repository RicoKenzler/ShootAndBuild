using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Attackable Target;
    public int YOffset = -20;

    Vector2 m_OriginalSize;

    // Use this for initialization
    void Start ()
    {
        m_OriginalSize = GetComponent<RectTransform>().sizeDelta;
	}
	
	// Update is called once per frame
	void Update ()
    {
        RectTransform rect = GetComponent<RectTransform>();

        Vector2 uiPos = RectTransformUtility.WorldToScreenPoint(Camera.main, Target.transform.position);
        uiPos.y += YOffset;
        rect.anchoredPosition = uiPos;

        float healthFactor = Target.Health / (float)Target.MaxHealth;

		Color colorFullHealth	= new Color(0.0f, 1.0f, 0.0f, 0.5f);
		Color colorMediumHealth	= new Color(1.0f, 1.0f, 0.0f, 0.5f);
		Color colorNoHealth		= new Color(1.0f, 0.0f, 0.0f, 0.5f);

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

        Vector2 size = new Vector2(m_OriginalSize.x * healthFactor, m_OriginalSize.y);
        rect.sizeDelta = size;
    }

}
