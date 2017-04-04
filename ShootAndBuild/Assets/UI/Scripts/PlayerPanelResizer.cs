using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelResizer : LayoutElement
{	
	RectTransform parentRectTransform;
	public float aspectRatio = 1.0f;

	private float GetDesiredWidth()
	{
		if (!parentRectTransform)
		{
			parentRectTransform = transform.parent.GetComponent<RectTransform>();
		}

		// TODO: no idea how i can make this field visible in inspector :(
		//const float aspectRatio = 1.35f;

		float height = parentRectTransform.rect.height;

		return height * aspectRatio;
	}

	public override float minWidth
	{
		get
		{
			return GetDesiredWidth();
		}

		set
		{
			base.minWidth = value;
		}
	}

	public override float preferredWidth
	{
		get
		{
			return GetDesiredWidth();
		}

		set
		{
			base.preferredWidth = value;
		}
	}
}
