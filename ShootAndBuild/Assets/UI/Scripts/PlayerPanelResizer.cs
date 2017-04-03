using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelResizer : LayoutElement
{	
	RectTransform rectTransform;

	public void InitWidths()
	{
		minWidth		= minWidth;
		preferredWidth	= preferredWidth;
	}

	private float GetDesiredWidth()
	{
		if (!rectTransform)
		{
			rectTransform = GetComponent<RectTransform>();
		}

		// TODO: no idea how i can make this field visible in inspector :(
		const float aspectRatio = 1.35f;

		float height	= rectTransform.offsetMax.y - rectTransform.offsetMin.y; 
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
