using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public class Canvas2D : MonoBehaviour 
	{
		[SerializeField] private GameObject m_NotificationBar;

		public static Canvas2D instance	{ get; private set; }
		public GameObject notificationBar { get { return m_NotificationBar; } }

		void Awake()
		{
			instance = this;
		}
	}
}
