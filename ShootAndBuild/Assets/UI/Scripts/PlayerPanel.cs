using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPanel : MonoBehaviour
{
	[SerializeField]
	GameObject healthBarFillImage;

	[SerializeField]
	GameObject activeItemImage;

	[SerializeField]
	GameObject activeWeaponImage;

    void Awake()
    {
        instance = this;
    }

	// Update is called once per frame
	void Update ()
	{
		
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
