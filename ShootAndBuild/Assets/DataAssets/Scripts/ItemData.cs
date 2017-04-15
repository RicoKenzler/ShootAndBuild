using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Custom/ItemData", fileName = "ItemData")]
public class ItemData : ScriptableObject
{
	public ItemType itemType				= ItemType.None;					
	public ItemUsageCategory usageCategory	= ItemUsageCategory.PassiveItem;	//< what should we do with usagePrefab on itemUsage?

	public GameObject usagePrefab;												//< which item should we throw/use/activate on itemUsage?

	public bool			isShared		= false;								//< does item go into shared inventory?
	public int			initialCount	= 0;									//< whith how many items does player start?
	public bool			useOnCollect	= false;								//< is the item used instantanously when collected?
	public Sprite		icon;													//< icon to show in UI
	
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}

