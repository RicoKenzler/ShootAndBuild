using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Attackable target;
    public int yOffset = -20;

    private Vector2 originalSize;


    void Start()
    {
        originalSize = GetComponent<RectTransform>().sizeDelta;
    }

    void Update()
    {
        RectTransform rect = GetComponent<RectTransform>();

        Vector2 uiPos = RectTransformUtility.WorldToScreenPoint(Camera.main, target.transform.position);
        uiPos.y += yOffset;
        rect.anchoredPosition = uiPos;

        float healthFactor = target.health / (float)target.maxHealth;

        Color colorFullHealth =     new Color(0.0f, 1.0f, 0.0f, 0.5f);
        Color colorMediumHealth =   new Color(1.0f, 1.0f, 0.0f, 0.5f);
        Color colorNoHealth =       new Color(1.0f, 0.0f, 0.0f, 0.5f);

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

        Vector2 size = new Vector2(originalSize.x * healthFactor, originalSize.y);
        rect.sizeDelta = size;
    }

}
