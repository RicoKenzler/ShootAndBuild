using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public class StorableItemData : MonoBehaviour
	{
        [SerializeField] private bool	m_IsShared	= false;            

		public bool	isShared { get { return m_IsShared; } }

		public bool CanBeUsedActively()
		{
			return (GetComponent<ConsumableData>() || GetComponent<ThrowableData>());
		}
	}
}