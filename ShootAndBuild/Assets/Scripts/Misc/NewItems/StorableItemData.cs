using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public class StorableItemData : MonoBehaviour
	{
        [SerializeField] private bool	m_IsShared	  = false;    
		[SerializeField] private string m_Abbreviation = "";

		public bool		isShared		{ get { return m_IsShared; } }
		public string	abbreviation	{ get { return m_Abbreviation; }}

		public bool CanBeUsedActively()
		{
			return (GetComponent<ConsumableData>() || GetComponent<ThrowableData>());
		}
	}
}