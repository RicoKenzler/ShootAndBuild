using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SAB
{
	public class ConsumableData : MonoBehaviour
	{
		[SerializeField] private int					m_HitPointDelta;
		[SerializeField] private bool					m_UseOnCollect = false;
		[SerializeField] private List<BuffData>			m_Buffs;

		public bool					useOnCollect	{ get { return m_UseOnCollect;  }}
		public int					hitPointDelta	{ get { return m_HitPointDelta; }}
		public List<BuffData>		buffs			{ get { return m_Buffs; } }

		///////////////////////////////////////////////////////////////////////////

		public void Consume(GameObject consumer)
		{
			// Hit Points
			if (hitPointDelta != 0)
			{
				Attackable attackable = consumer.GetComponent<Attackable>();

				if (hitPointDelta > 0)
				{
					attackable.Heal(hitPointDelta);
				}
				else
				{
					attackable.DealDamage(-m_HitPointDelta, consumer, consumer);
				}
			}

			// Buffs
			if (m_Buffs.Count > 0)
			{
				Buffable buffable = consumer.GetComponent<Buffable>();
				buffable.AddBuffs(m_Buffs);
			}

			// Player Counter
			InputController inputController = consumer.GetComponent<InputController>();

			if (inputController)
			{
				CounterManager.instance.AddToCounters(inputController.playerID, CounterType.ItemsUsed, 1, gameObject.ToString());
			}
		}
	}
}