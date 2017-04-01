using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using UnityEngine;

public class PlayerPanel : MonoBehaviour
{
	[SerializeField]
	Image healthBarFillImage;

	[SerializeField]
	Image activeItemImage;

	[SerializeField]
	Image activeWeaponImage;

    void Awake()
    {
        instance = this;
    }

	// Update is called once per frame
	void Update ()
	{
		healthBarFillImage.color = new Color(1.0f, 1.0f, (Time.time * 0.5f) % 2.0f);
		healthBarFillImage.fillAmount = (Time.time * 0.5f) % 2.0f;
	}

	public int health
    {
        get; set;
    }

	public GameObject activeItem
    {
        get; set;
    }

	public GameObject activeWeapon
    {
        get; set;
    }

	public static PlayerPanel instance
    {
        get; set;
    }
}
